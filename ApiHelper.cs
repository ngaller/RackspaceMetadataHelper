using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.openstack.Core.Domain;
using net.openstack.Providers.Rackspace;

namespace RackspaceMetadataHelper
{
    public class ApiHelper
    {
        public static IEnumerable<String> ListContainers(string username, string apiKey)
        {
            var filesService = GetFilesService(username, apiKey);
            var conts = filesService.ListContainers();
            return conts.Select(x => x.Name);
        }

        public static int ApplyObjectHeader(string username, string apiKey, string containerName, string headerKey, string headerValue, Action<int, int> progressReporter)
        {
            var filesService = GetFilesService(username, apiKey);
            var container = filesService.GetContainerHeader(containerName);
            var total = int.Parse(container["X-Container-Object-Count"]);
            if (total > 10000)
                throw new Exception("Unable to set header on container with more than 10,000 items");
            // list objects will return up to 10000 items
            var objects = filesService.ListObjects(containerName);
            int modifiedCount = 0, current = 0;
            foreach (var obj in objects)
            {
                current++;
                progressReporter(current, total);
                var headers = filesService.GetObjectHeaders(containerName, obj.Name);
                if (headers.ContainsKey(headerKey) && headers[headerKey] == headerValue)
                    continue;

                // remove some headers that can't be set explicitly   
                headers.Remove("X-Timestamp");
                headers.Remove("X-Trans-Id");
                headers.Remove("Accept-Ranges");
                headers.Remove("Content-Length");
                headers.Remove("Content-Type");
                headers.Remove("Date");
                headers.Remove("ETag");
                headers.Remove("Last-Modified");

                headers[headerKey] = headerValue;
                filesService.UpdateObjectHeaders(containerName, obj.Name, headers);
                modifiedCount++;
            }
            return modifiedCount;
        }

        private static CloudFilesProvider GetFilesService(string username, string apiKey)
        {
            var identity = new CloudIdentity
            {
                Username = username,
                APIKey = apiKey
            };
            var identityService = new CloudIdentityProvider(identity);
            var filesService = new CloudFilesProvider(identity, identityService);
            return filesService;
        }
    }
}
