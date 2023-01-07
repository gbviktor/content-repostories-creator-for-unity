using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using MontanGames.Data.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Rendering;

namespace MontanaGames
{
    [CreateAssetMenu(menuName = GameEditorSettings.AssetsBaseURL + "/" + nameof(GameContentSystem), fileName = nameof(GameContentSystem))]
    public class GameContentSystem : ScriptableObject
    {
        [SerializeField] private RepositoriesManager repos;
        [SerializeField] private SerializedDictionary<Type, byte> types = new SerializedDictionary<Type, byte>();
        [SerializeField] private SerializedDictionary<Type, GameContentType> gameSystemTypesOrg = new SerializedDictionary<Type, GameContentType>();

        [SerializeField] RepositoriesGenerator repositoriesGenerator;

        public RepositoriesManager Repos => repos;
        public byte GetTypeID<T>()
        {
            return types[typeof(T)];
        }

        #region UNITY_EDITOR
#if UNITY_EDITOR

        #region Processing

        void OnValidate()
        {
            FindAllRegistredContentTypes();
            FindRepositories();
        }
        private void FindRepositories()
        {
            if (repos == null) return;

            repos.FindRepositoriesInProject();
            if (repos.RepositoriesCount() != gameSystemTypesOrg.Count)
            {
                repositoriesGenerator.ForceGenerate(gameSystemTypesOrg.Values.ToList());
            }
        }
        private void FindAllRegistredContentTypes()
        {
            types.Clear();
            gameSystemTypesOrg.Clear();

            var list = RegisterTypeAttribute.GetListOfRegistredTypes();
            list.ForEach(x => ForceRegisterType(x.RegisteredType));
        }
        #endregion

        #region Attribute Functions
        private void ForceRegisterType(GameContentType registeredType)
        {
            types.Add(registeredType.RegistredType, registeredType.TypeUniqueID);

            gameSystemTypesOrg.Add(registeredType.RegistredType, registeredType);
            CheckConflicts();
        }

        private void CheckConflicts()
        {
            foreach (var pair in types)
            {
                if (CheckConflict(pair.Key, pair.Value, out var conflicted))
                {
                    Debug.LogError($"Conflict found for Type {pair.Key}:{pair.Value} with {conflicted.RegisteredType.RegistredType}:{conflicted.RegisteredType.TypeUniqueID}");
                }
            }
        }

        private bool CheckConflict(Type registredType, byte typeUniqueID, out RegisterTypeAttribute conflicted)
        {
            if (types.ContainsValue(typeUniqueID))
            {
                foreach (var pair in types)
                {
                    if (pair.Value == typeUniqueID && pair.Key != registredType)
                    {
                        var conflictedWithType = pair.Key.GetCustomAttributes<RegisterTypeAttribute>().FirstOrDefault();
                        if (conflictedWithType == null || conflictedWithType.RegisteredType.RegistredType == registredType)
                            continue;

                        if (conflictedWithType.RegisteredType.TypeUniqueID == typeUniqueID)
                        {
                            conflicted = conflictedWithType;
                            return true;
                        }
                    }
                }
            }
            conflicted = default;
            return false;
        }
        #endregion

        #region Instatance & Scriptable Object
        static GameContentSystem _instance;
        public static GameContentSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAsset<GameContentSystem>();
                };

                return _instance;
            }
        }
        static T FindAsset<T>() where T : UnityEngine.Object
        {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { "Assets/" });

            foreach (string guid in assetGuids)
            {
                var s = (T)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(T));
                return s;
            }
            return null;
        }
        public static void SelectInProjectOrCreateNew<T>(ref T instance) where T : ScriptableObject
        {
            if (instance != null)
                ProjectWindowUtil.ShowCreatedAsset(instance);
            else
            {
                instance = ScriptableObject.CreateInstance<T>();
                ProjectWindowUtil.CreateAsset(instance, $"{typeof(T)}" + ".asset");
            }
        }
        #endregion
#endif
        #endregion
    }

    public interface IBinderRepository
    {
        public byte Type { get; }
        public Type GetTypeOfData();
    }

    public interface IBinderRepository<T> : IBinderRepository
    {
        T AddOrUpdate(IBindID bindID, T data);
        IEnumerable<T> GetAll();
        bool Get(IBindID bindID, out T data);
        bool Delete(IBindID bindID);
    }

    public interface IDataTypeID
    {
        public byte Type { get; }
    }

    #region Game System Attribute Helpers
    [Serializable]
    public class GameContentType
    {
        [SerializeField] private Type registredType;
        [SerializeField] private byte typeUniqueID;
        [SerializeField] private bool isHelperType;

        public GameContentType(Type registredType, byte typeUniqueID, bool isHelperType = false)
        {
            this.registredType = registredType;
            this.typeUniqueID = typeUniqueID;
            this.isHelperType = isHelperType;
        }

        public byte TypeUniqueID => typeUniqueID;
        public Type RegistredType => registredType;
        public bool IsHelperType => isHelperType;

    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class RegisterTypeAttribute : Attribute
    {
        public GameContentType RegisteredType { get; private set; }
        public RegisterTypeAttribute(Type registredType, byte typeUniqueID, bool isHelperType = false)
        {
#if UNITY_EDITOR
            RegisteredType = new GameContentType(registredType, typeUniqueID, isHelperType);
#endif
        }

        public static List<RegisterTypeAttribute> GetListOfRegistredTypes()
        {
            List<RegisterTypeAttribute> list = new List<RegisterTypeAttribute>();
            var extracted = TypeCache.GetTypesWithAttribute<RegisterTypeAttribute>();

            foreach (var m in extracted)
            {
                var att = m.GetCustomAttribute<RegisterTypeAttribute>();
                if (att != null)
                    list.Add(att);
            }
            return list;
        }
    }
    #endregion
}
