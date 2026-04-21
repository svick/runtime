// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting.Internal
{
    internal sealed class ServiceFactoryAdapter<TContainerBuilder> : IServiceFactoryAdapter where TContainerBuilder : notnull
    {
        private IServiceProviderFactory<TContainerBuilder>? _serviceProviderFactory;
        private readonly Func<HostBuilderContext>? _contextResolver;
        private readonly Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>>? _factoryResolver;

        public ServiceFactoryAdapter(IServiceProviderFactory<TContainerBuilder> serviceProviderFactory)
        {
            ArgumentNullException.ThrowIfNull(serviceProviderFactory);

            _serviceProviderFactory = serviceProviderFactory;
        }

        public ServiceFactoryAdapter(Func<HostBuilderContext> contextResolver, Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factoryResolver)
        {
            ArgumentNullException.ThrowIfNull(contextResolver);
            ArgumentNullException.ThrowIfNull(factoryResolver);

            _contextResolver = contextResolver;
            _factoryResolver = factoryResolver;
        }

        public object CreateBuilder(IServiceCollection services)
        {
            if (_serviceProviderFactory == null)
            {
                Debug.Assert(_factoryResolver != null && _contextResolver != null);
                Console.WriteLine($"[OOM-TRACE] ServiceFactoryAdapter.CreateBuilder: resolving context | GC={GC.GetTotalMemory(false):N0} WS={Environment.WorkingSet:N0}");
                Console.Out.Flush();
                var context = _contextResolver();
                Console.WriteLine($"[OOM-TRACE] ServiceFactoryAdapter.CreateBuilder: calling factory resolver | GC={GC.GetTotalMemory(false):N0} WS={Environment.WorkingSet:N0}");
                Console.Out.Flush();
                _serviceProviderFactory = _factoryResolver(context);

                if (_serviceProviderFactory == null)
                {
                    throw new InvalidOperationException(SR.ResolverReturnedNull);
                }
                Console.WriteLine($"[OOM-TRACE] ServiceFactoryAdapter.CreateBuilder: factory resolved | GC={GC.GetTotalMemory(false):N0} WS={Environment.WorkingSet:N0}");
                Console.Out.Flush();
            }
            Console.WriteLine($"[OOM-TRACE] ServiceFactoryAdapter.CreateBuilder: calling CreateBuilder on factory | GC={GC.GetTotalMemory(false):N0} WS={Environment.WorkingSet:N0}");
            Console.Out.Flush();
            return _serviceProviderFactory.CreateBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(object containerBuilder)
        {
            if (_serviceProviderFactory == null)
            {
                throw new InvalidOperationException(SR.CreateBuilderCallBeforeCreateServiceProvider);
            }

            Console.WriteLine($"[OOM-TRACE] ServiceFactoryAdapter.CreateServiceProvider: calling factory | GC={GC.GetTotalMemory(false):N0} WS={Environment.WorkingSet:N0}");
            Console.Out.Flush();
            return _serviceProviderFactory.CreateServiceProvider((TContainerBuilder)containerBuilder);
        }
    }
}
