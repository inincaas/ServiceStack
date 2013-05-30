﻿namespace Owin
{
    using ServiceStack.Owin.Infrastructure;
    using ServiceStack.WebHost.Endpoints;

    public static class OwinExtensions
    {
        public static void UseServiceStack(this IAppBuilder appbuilder, AppHostBase appHostBase)
        {
            appbuilder.Use(typeof(AppHostDistpatcherHandler), appHostBase);
        }
    }
}