using System;
using System.Collections.Generic;

namespace ParkingOut.Services
{
    /// <summary>
    /// Simple service locator for dependency injection
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service implementation
        /// </summary>
        /// <typeparam name="TInterface">The service interface type</typeparam>
        /// <param name="implementation">The service implementation</param>
        public static void RegisterService<TInterface>(TInterface implementation) where TInterface : class
        {
            _services[typeof(TInterface)] = implementation;
        }

        /// <summary>
        /// Gets a registered service
        /// </summary>
        /// <typeparam name="TInterface">The service interface type</typeparam>
        /// <returns>The service implementation</returns>
        public static TInterface GetService<TInterface>() where TInterface : class
        {
            if (_services.TryGetValue(typeof(TInterface), out var service))
            {
                return (TInterface)service;
            }

            // If service not registered, create default implementation if possible
            if (typeof(TInterface) == typeof(IVehicleEntryService))
            {
                var implementation = new Implementations.VehicleEntryService();
                RegisterService<TInterface>((TInterface)(object)implementation);
                return (TInterface)(object)implementation;
            }
            else if (typeof(TInterface) == typeof(IVehicleExitService))
            {
                var entryService = GetService<IVehicleEntryService>();
                var implementation = new Implementations.VehicleExitService(entryService);
                RegisterService<TInterface>((TInterface)(object)implementation);
                return (TInterface)(object)implementation;
            }

            throw new InvalidOperationException($"Service of type {typeof(TInterface).Name} is not registered");
        }

        /// <summary>
        /// Clears all registered services
        /// </summary>
        public static void ClearServices()
        {
            _services.Clear();
        }
    }
}