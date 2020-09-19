﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Arashi.Kestrel;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;
using Microsoft.AspNetCore.Http;

namespace Arashi
{
    public class DnsCache
    {
        public static void Add(DnsMessage dnsMessage)
        {
            if (dnsMessage.AnswerRecords.Count <= 0) return;
            var dnsRecordBase = dnsMessage.AnswerRecords.FirstOrDefault();
            Add(new CacheItem($"DNS:{dnsRecordBase.Name}:{dnsRecordBase.RecordType}",
                dnsMessage.AnswerRecords.ToList()), dnsRecordBase.TimeToLive);
        }

        public static void Add(DnsMessage dnsMessage, HttpContext context)
        {
            if (dnsMessage.AnswerRecords.Count <= 0) return;
            var dnsRecordBase = dnsMessage.AnswerRecords.FirstOrDefault();
            if (RealIP.TryGetFromDns(dnsMessage, out var ipAddress))
                Add(new CacheItem(
                    $"DNS:{GeoIP.GetGeoStr(ipAddress)}{dnsRecordBase.Name}:{dnsRecordBase.RecordType}",
                    dnsMessage.AnswerRecords.ToList()), dnsRecordBase.TimeToLive);
            else
                Add(new CacheItem($"DNS:{dnsRecordBase.Name}:{dnsRecordBase.RecordType}",
                    dnsMessage.AnswerRecords.ToList()), dnsRecordBase.TimeToLive);
        }

        public static void Add(CacheItem cacheItem, int ttl)
        {
            if (!MemoryCache.Default.Contains(cacheItem.Key))
                MemoryCache.Default.Add(cacheItem,
                    new CacheItemPolicy
                    {
                        AbsoluteExpiration =
                            DateTimeOffset.Now + TimeSpan.FromSeconds(ttl)
                    });
        }

        public static bool Contains(DnsMessage dnsQMsg, HttpContext context = null)
        {
            return context == null
                ? MemoryCache.Default.Contains(
                    $"DNS:{dnsQMsg.Questions.FirstOrDefault().Name}:{dnsQMsg.Questions.FirstOrDefault().RecordType}")
                : MemoryCache.Default.Contains(
                    $"DNS:{GeoIP.GetGeoStr(RealIP.GetFromDns(dnsQMsg, context))}{dnsQMsg.Questions.FirstOrDefault().Name}:{dnsQMsg.Questions.FirstOrDefault().RecordType}");
        }

        public static DnsMessage Get(DnsMessage dnsQMessage, HttpContext context = null)
        {
            var dCacheMsg = new DnsMessage
            {
                IsRecursionAllowed = true,
                IsRecursionDesired = true,
                TransactionID = dnsQMessage.TransactionID
            };
            if (context != null)
                dCacheMsg.AnswerRecords.AddRange(Get(
                    $"DNS:{GeoIP.GetGeoStr(RealIP.GetFromDns(dnsQMessage, context))}{dnsQMessage.Questions.FirstOrDefault().Name}:{dnsQMessage.Questions.FirstOrDefault().RecordType}"));
            else
                dCacheMsg.AnswerRecords.AddRange(Get(
                    $"DNS:{dnsQMessage.Questions.FirstOrDefault().Name}:{dnsQMessage.Questions.FirstOrDefault().RecordType}"));
            dCacheMsg.Questions.AddRange(dnsQMessage.Questions);
            dCacheMsg.AnswerRecords.Add(new TxtRecord(DomainName.Parse("cache.doh.pp.ua"), 0,
                "ArashiDNS.P Cached"));
            return dCacheMsg;
        }

        public static List<DnsRecordBase> Get(string key)
        {
            return (List<DnsRecordBase>) MemoryCache.Default.Get(key) ??
                   throw new InvalidOperationException();
        }
    }
}
