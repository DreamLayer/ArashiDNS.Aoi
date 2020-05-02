﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Arashi.Kestrel
{
    public class RealIP
    {
        public static string Get(HttpContext context)
        {
            try
            {
                var request = context.Request;
                if (request.Headers.ContainsKey("CF-Connecting-IP"))
                    return request.Headers["CF-Connecting-IP"].ToString();
                if (request.Headers.ContainsKey("X-Real-IP"))
                    return request.Headers["X-Real-IP"].ToString();
                if (request.Headers.ContainsKey("X-Real-IP"))
                    return request.Headers["X-Real-IP"].ToString();
                if (request.Headers.ContainsKey("X-Forwarded-For"))
                    return (request.Headers["X-Forwarded-For"].ToString().Split(',', ':').FirstOrDefault().Trim());
                return context.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return context.Connection.RemoteIpAddress.ToString();
            }
        }
    }
}
