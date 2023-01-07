
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEditor;

using UnityEngine;

namespace MontanaGames
{
    [CreateAssetMenu(menuName = GameEditorSettings.AssetsBaseURL + "/Tools/" + nameof(RepositoriesGenerator), fileName = nameof(RepositoriesGenerator))]
    public class RepositoriesGenerator : ScriptableObject
    {
        [SerializeField] private string pathToGenerated;

        #region UNITY EDITOR
#if UNITY_EDITOR
        [MenuItem("Force Generate")]
        public void ForceGenerate(List<GameContentType> list)
        {
            foreach (GameContentType type in list)
            {
                ForceGenerate(type);
            }
        }

        private void ForceGenerate(GameContentType type)
        {
            var fileName = $"{type.RegistredType.Name}Repository.cs";

            var generated = RepositorySourceTemplate
                .Replace("$type", type.RegistredType.Name)
                .Replace("$IDtype", type.TypeUniqueID.ToString());


            var pathToTemplateScript = AssetDatabase.GetAssetPath(this);
            var pathToFolder = pathToTemplateScript.Remove(pathToTemplateScript.LastIndexOf("/"), pathToTemplateScript.Length - pathToTemplateScript.LastIndexOf("/"));
            var pathSaveWithFileName = $"{pathToFolder}/{fileName}";

            byte[] bytes = Encoding.UTF8.GetBytes(generated);

            Debug.Log(Application.dataPath.Replace("Assets", "") + pathSaveWithFileName);
            File.WriteAllBytes(Application.dataPath.Replace("Assets", "") + pathSaveWithFileName, bytes);
        }

        private void OnValidate()
        {
            pathToGenerated = AssetDatabase.GetAssetPath(this);
        }

        const string RepositorySourceTemplate = @"
//This Source Code is generated, don't try make changes here
//Use instead partial class of this Repository to extend functional
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MontanGames.Data.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace MontanaGames.Experemental
{
    [CreateAssetMenu(menuName = ""Montana Games/DB/$type Repository"", fileName = ""$typeRepository"")]
    public class $typeRepository : ScriptableObject, IBinderRepository, IBinderRepository<$type>
    {
        [SerializeField] readonly byte type = $IDtype;
        public byte Type => type;

        [SerializeField, JsonProperty(""repos"")] public SerializedDictionary<string, $type> repos = new SerializedDictionary<string, $type>();

        public $type AddOrUpdate(IBindID bindID, $type data)
        {
            if (Get(bindID, out var existed))
            {
                Replace(bindID, data);

            } else
            {
                repos.Add(bindID.ToEntityID(), data);
            }
            return data;
        }
        public IEnumerable<$type> GetAll()
        {
            return repos.Values;
        }
        private void Replace(IBindID bindID, $type data)
        {
            repos[bindID.ToEntityID()] = data;
        }

        public bool GetData(IBindID bindID, out $type data)
        {
            var res = repos.TryGetValue(bindID.ToEntityID(), out var binder);
            data = binder;
            return res;
        }
        public bool Get(IBindID bindID, out $type data)
        {
            return repos.TryGetValue(bindID.ToEntityID(), out data);
        }

        public bool Delete(IBindID bindID)
        {
            return repos.Remove(bindID.ToEntityID());
        }

        public Type GetTypeOfData()
        {
            return typeof($type);
        }
    }
}
";
#endif
        #endregion
    }
}