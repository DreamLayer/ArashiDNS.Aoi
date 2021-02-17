﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ARSoft.Tools.Net;
using ARSoft.Tools.Net.Dns;

namespace Arashi
{
    public static class DnsEncoder
    {
        private static MethodInfo info;

        public static void Init()
        {
            Parallel.ForEach(new DnsMessage().GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), mInfo =>
            {
                if (mInfo.ToString() == "Int32 Encode(Boolean, Byte[] ByRef)")
                    info = mInfo;
            });
        }

        public static byte[] Encode(DnsMessage dnsMsg)
        {
            if (info == null) Init();

            dnsMsg.IsRecursionAllowed = true;
            dnsMsg.IsRecursionDesired = true;
            dnsMsg.IsQuery = false;
            dnsMsg.TransactionID = 0;
            dnsMsg.IsEDnsEnabled = false;
            dnsMsg.AdditionalRecords.Clear();


            foreach (var item in new List<DnsRecordBase>(dnsMsg.AnswerRecords).Where(item =>
                item.Name.IsSubDomainOf(DomainName.Parse("arashi-msg")) ||
                item.Name.IsSubDomainOf(DomainName.Parse("nova-msg"))))
                dnsMsg.AnswerRecords.Remove(item);

            //if (dnsBytes != null && dnsBytes[2] == 0) dnsBytes[2] = 1;
            var args = new object[] {false, null};
            if (info != null) info.Invoke(dnsMsg, args);
            return bytesTrimEnd(args[1] as byte[]);
        }

        private static byte[] bytesTrimEnd(byte[] bytes)
        {
            var list = bytes.ToList();
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] == 0x00)
                    list.RemoveAt(i);
                else
                    break;
            }
            return list.ToArray();
        }
    }
}
