using System;
using System.Collections.Generic;

namespace MazeMath.Core
{
    public sealed class GameServices
    {
        private readonly Dictionary<Type, object> services = new();

        public static GameServices Instance { get; private set; }

        public static GameServices Ensure()
        {
            return Instance ??= new GameServices();
        }

        public void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            services[typeof(T)] = service;
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out var value) && value is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }

        public T Get<T>() where T : class
        {
            return TryGet<T>(out var service) ? service : null;
        }
    }
}
