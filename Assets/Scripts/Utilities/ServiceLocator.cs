using System;
using System.Collections.Generic;

public interface IService
{
	void Init();
	void Shutdown();
}

public class ServiceLocator
{
	private static Dictionary<Type, IService> _services;

	public static void Register<T>(T service) where T : IService
	{
		if (_services == null)
		{
			_services = new Dictionary<Type, IService>();
		}

		_services.Add(typeof(T), service);
	}

	public static T Get<T>() where T : IService
	{
		return (T) _services[typeof(T)];
	}

	public static void InitAllServices()
	{
		if (_services != null)
		{
			foreach (var service in _services.Values)
			{
				service.Init();
			}
		}
	}

	public static void Shutdown()
	{
		if (_services != null)
		{
			foreach (var service in _services.Values)
			{
				service.Shutdown();
			}
		}

		_services.Clear();
		_services = null;
	}
}