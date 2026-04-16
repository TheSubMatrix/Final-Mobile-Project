using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MatrixUtils.DependencyInjection {

    [DefaultExecutionOrder(-1000)]
    public class Injector : MonoBehaviour, IInjector, IDependencyProvider
    {
        [Provide] IInjector GetInjector() => this;
        const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        readonly Dictionary<Type, object> m_registry = new();

        protected void Awake()
        {
	        MonoBehaviour[] monoBehaviours = FindMonoBehaviours();

	        // Find and register all providers
	        IEnumerable<IDependencyProvider> providers = monoBehaviours.OfType<IDependencyProvider>();
	        foreach (IDependencyProvider provider in providers) {
		        if (provider is MonoBehaviour mb && mb == null) continue;
		        Register(provider);
	        }

	        // Inject into all injectables
	        IEnumerable<MonoBehaviour> injectables = monoBehaviours.Where(IsInjectable);
	        foreach (MonoBehaviour injectable in injectables) {
		        InjectInternal(injectable);
	        }
        }

        void InjectInternal(object instance)
        {
            Type type = instance.GetType();

            if (instance is MonoBehaviour mb && mb == null) return;

            IEnumerable<FieldInfo> injectableFields = type.GetFields(BindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (FieldInfo injectableField in injectableFields) {
                object currentValue = injectableField.GetValue(instance);

                if (currentValue != null && (currentValue is not UnityEngine.Object unityObj || unityObj != null))
                {
                    continue;
                }

                Type fieldType = injectableField.FieldType;
                object resolvedInstance = Resolve(fieldType);

                if (resolvedInstance == null) {
                    throw new($"Failed to inject {fieldType.Name} into {type.Name}.{injectableField.Name}");
                }
                injectableField.SetValue(instance, resolvedInstance);
            }

            IEnumerable<MethodInfo> injectableMethods = type.GetMethods(BindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (MethodInfo injectableMethod in injectableMethods) {
                Type[] requiredParameters = injectableMethod.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();
                object[] resolvedInstances = requiredParameters.Select(Resolve).ToArray();
                if (resolvedInstances.Any(resolvedInstance => resolvedInstance == null)) {
                    throw new($"Failed to inject into method {type.Name}.{injectableMethod.Name}");
                }

                injectableMethod.Invoke(instance, resolvedInstances);
            }

            IEnumerable<PropertyInfo> injectableProperties = type.GetProperties(BindingFlags)
                .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

            foreach (PropertyInfo injectableProperty in injectableProperties) {
                Type propertyType = injectableProperty.PropertyType;
                object resolvedInstance = Resolve(propertyType);

                if (resolvedInstance == null) {
                    throw new($"Failed to inject {propertyType.Name} into {type.Name}.{injectableProperty.Name}");
                }

                injectableProperty.SetValue(instance, resolvedInstance);
            }

            #if UNITY_EDITOR
            if (instance is UnityEngine.Object unityObjInstance) {
                UnityEditor.EditorUtility.SetDirty(unityObjInstance);
            }
            #endif
        }

        public void Register<T>(T instance)
        {
	        m_registry[typeof(T)] = instance;
        }

        void Register(IDependencyProvider provider) {
            MethodInfo[] methods = provider.GetType().GetMethods(BindingFlags);

            foreach (MethodInfo method in methods) {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;

                Type returnType = method.ReturnType;
                object providedInstance = method.Invoke(provider, null);

                if (providedInstance == null) continue;

                m_registry[returnType] = providedInstance;
            }
        }

        object Resolve(Type type) {
            m_registry.TryGetValue(type, out object resolvedInstance);
            return resolvedInstance;
        }

        public static void ValidateDependencies() {
            MonoBehaviour[] monoBehaviours = FindMonoBehaviours();
            IEnumerable<IDependencyProvider> providers = monoBehaviours.OfType<IDependencyProvider>();
            HashSet<Type> providedDependencies = GetProvidedDependencies(providers);

            IEnumerable<string> invalidDependencies = monoBehaviours
                .SelectMany(mb => mb.GetType().GetFields(BindingFlags), (mb, field) => new {mb, field})
                .Where(t => Attribute.IsDefined(t.field, typeof(InjectAttribute)))
                .Where(t => !providedDependencies.Contains(t.field.FieldType) && t.field.GetValue(t.mb) == null)
                .Select(t => $"[Validation] {t.mb.GetType().Name} is missing dependency {t.field.FieldType.Name} on GameObject {t.mb.gameObject.name}");

            List<string> invalidDependencyList = invalidDependencies.ToList();

            if (!invalidDependencyList.Any()) {
                Debug.Log("[Validation] All dependencies are valid.");
            } else {
                Debug.LogError($"[Validation] {invalidDependencyList.Count} dependencies are invalid:");
                foreach (string invalidDependency in invalidDependencyList) {
                    Debug.LogError(invalidDependency);
                }
            }
        }

        static HashSet<Type> GetProvidedDependencies(IEnumerable<IDependencyProvider> providers) {
            HashSet<Type> providedDependencies = new();
            foreach (Type returnType in providers
                         .Select(provider => provider.GetType().GetMethods(BindingFlags))
                         .SelectMany(methods => from method in methods
                             where Attribute.IsDefined(method, typeof(ProvideAttribute))
                             select method.ReturnType))
            {
                providedDependencies.Add(returnType);
            }

            return providedDependencies;
        }

        public static void ClearDependencies() {
            foreach (MonoBehaviour monoBehaviour in FindMonoBehaviours()) {
                Type type = monoBehaviour.GetType();
                IEnumerable<FieldInfo> injectableFields = type.GetFields(BindingFlags)
                    .Where(member => Attribute.IsDefined(member, typeof(InjectAttribute)));

                foreach (FieldInfo injectableField in injectableFields) {
                    injectableField.SetValue(monoBehaviour, null);
                }
            }

            Debug.Log("[Injector] All injectable fields cleared.");
        }

        static MonoBehaviour[] FindMonoBehaviours() {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }

        static bool IsInjectable(MonoBehaviour obj) {
            MemberInfo[] members = obj.GetType().GetMembers(BindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(InjectAttribute)));
        }

        public void Inject(MonoBehaviour injectable)
        {
            if (!IsInjectable(injectable))
            {
                return;
            }
            InjectInternal(injectable);
        }
    }
}