using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace MontanaGames
{
    [CreateAssetMenu(menuName = GameEditorSettings.AssetsBaseURL + "/" + nameof(RepositoriesManager), fileName = nameof(RepositoriesManager))]

    public class RepositoriesManager : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<byte, ScriptableObject> repositories = new SerializedDictionary<byte, ScriptableObject>();


        public IBinderRepository<T> GetRepository<T>()
        {
            var tid = GameContentSystem.Instance.GetTypeID<T>();
            return (IBinderRepository<T>)repositories[tid];
        }

        #region UNITY EDITOR
#if UNITY_EDITOR

        [InitializeOnLoadMethod]
        private void OnValidate()
        {
            FindRepositoriesInProject();
        }
        List<T> FindAssets<T>(string pathIntoAssets, in List<T> list)
        {
            string[] dataWrappersResult = AssetDatabase.FindAssets($"t:ScriptableObject", new string[] { pathIntoAssets });

            foreach (string guid in dataWrappersResult)
            {
                var s = LoadScriptableFromGuid(guid);

                if (s is T obj)
                {
                    list.Add(obj);
                }
            }
            return list;

            ScriptableObject LoadScriptableFromGuid(string guid)
            {
                return (ScriptableObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(ScriptableObject));
            }
        }
        internal void FindRepositoriesInProject()
        {
            List<IBinderRepository> listOfRepositories = new List<IBinderRepository>();
            var allScriptables = FindAssets("Assets/", in listOfRepositories);
            ClearRepositoriesList();
            listOfRepositories.ForEach(x => RegisterRepository(x));
        }

        private void ClearRepositoriesList()
        {
            repositories.Clear();
        }

        private void RegisterRepository(IBinderRepository x)
        {
            repositories.Add(x.Type, x as ScriptableObject);
        }

        internal int RepositoriesCount()
        {
            return repositories.Count;
        }
#endif
        #endregion
    }
}