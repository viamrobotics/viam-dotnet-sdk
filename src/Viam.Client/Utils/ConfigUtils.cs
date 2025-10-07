using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Viam.Client.Utils
{
    public static class ConfigUtils
    {
        private const string DefaultConfigPath = "/etc/viam.json";
        private const string CloudConfigPath = "/root/.viam/";

        private static string CloudConfigFileName(string id) =>
            Path.Combine(CloudConfigPath, $"cached_cloud_config_{id}.json");

        public static (string ApiKeyId, string ApiKey) GetCredentialsFromConfig()
        {
            var config = GetMachineConfig();
            if (config["auth"] is null) throw new InvalidDataException("No auth section found in config");
            var auth = config["auth"]!;
            if (auth["handlers"] is null) throw new InvalidDataException("No auth.handlers section found in config");
            if (auth["handlers"]!.GetType() != typeof(System.Text.Json.Nodes.JsonArray)) throw new InvalidDataException("auth.handlers it not an array");
            var handlers = auth["handlers"]!.AsArray();
            var apiKeyHandlers = handlers.Where(x => x?["type"] != null)
                .Where(x => x!["type"]!.GetValue<string>() is "api-key").ToArray();

            if (apiKeyHandlers.Length == 0)
                throw new InvalidDataException("No api-key handler found in auth.handlers");
            if (apiKeyHandlers.Length > 1)
                Debug.WriteLine("Multiple api-key handlers found, using the first handler");
            
            var handler = apiKeyHandlers[0]!;
            if (handler["config"] is null) throw new InvalidDataException("No config section found in api-key handler");
            var handlerConfig = handler["config"]!;
            if (handlerConfig!["keys"] is null)throw new InvalidDataException("No keys section found in api-key handler config");
            if (handlerConfig["keys"]!.GetType() != typeof(JsonArray)) throw new InvalidDataException("auth.handlers.config.keys is not an array");
            
            var keys = handlerConfig["keys"]!.AsArray();
            if (keys.Count == 0) throw new InvalidDataException("No keys found in api-key handler config");
            if (keys.Count > 1) Debug.WriteLine("Multiple keys found in api-key handler config, using the first key");
            
            var keyId = keys[0]!.GetValue<string>();
            if (handlerConfig[keyId] is null) throw new InvalidDataException($"No key_id found in api-key handler config");
            var key = handlerConfig[keyId]!.GetValue<string>();
            
            return (keyId, key);
        }

        public static string GetMachineId()
        {
            var config = GetMachineConfig();
            if (config["cloud"] is null) throw new InvalidDataException("No cloud field found in config");
            var cloud = config["cloud"]!;
            if (cloud["machine_id"] is null) throw new InvalidDataException("No cloud.machine_id field found in config");
            var id = cloud["machine_id"]!.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id)) throw new InvalidDataException("cloud.machine_id field in config is empty");
            return id;
        }

        public static string GetMachinePartId()
        {
            var config = GetMachineConfig();
            if (config["cloud"] is null) throw new InvalidDataException("No cloud field found in config");
            var cloud = config["cloud"]!;
            if (cloud["id"] is null) throw new InvalidDataException("No cloud.id field found in config");
            var id = cloud["id"]!.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id)) throw new InvalidDataException("cloud.id field in config is empty");
            return id;
        }

        public static string GetMachineFqdn()
        {
            var config = GetMachineConfig();
            if (config["cloud"] is null) throw new InvalidDataException("No cloud field found in config");
            var cloud = config["cloud"]!;
            if (cloud["fqdn"] is null) throw new InvalidDataException("No cloud.fqdn field found in config");
            var fqdn = cloud["fqdn"]!.GetValue<string>();
            if (string.IsNullOrWhiteSpace(fqdn)) throw new InvalidDataException("cloud.fqdn field in config is empty");
            return fqdn;
        }

        public static string GetMachineLocalFqdn()
        {
            var config = GetMachineConfig();
            if (config["cloud"] is null) throw new InvalidDataException("No cloud field found in config");
            var cloud = config["cloud"]!;
            if (cloud["local_fqdn"] is null) throw new InvalidDataException("No cloud.local_fqdn field found in config");
            var localFqdn = cloud["local_fqdn"]!.GetValue<string>();
            if (string.IsNullOrWhiteSpace(localFqdn)) throw new InvalidDataException("cloud.local_fqdn field in config is empty");
            return localFqdn;
        }

        public static string GetMachineSignalingAddress()
        {
            var config = GetMachineConfig();
            if (config["cloud"] is null) throw new InvalidDataException("No cloud field found in config");
            var cloud = config["cloud"]!;
            if (cloud["signaling_address"] is null) throw new InvalidDataException("No cloud.signaling_address field found in config");
            var signalingAddress = cloud["signaling_address"]!.GetValue<string>();
            if (string.IsNullOrWhiteSpace(signalingAddress)) throw new InvalidDataException("cloud.signaling_address field in config is empty");
            return signalingAddress;
        }

        public static JsonNode GetMachineConfig()
        {
            var cfgFilePath = GetMachineConfigPath();
            var cfgText = File.ReadAllText(cfgFilePath);
            var json = System.Text.Json.JsonSerializer.Deserialize<JsonNode>(cfgText);
            if (json == null)
                throw new InvalidDataException($"No JSON data found in config file at {cfgFilePath}");
            return json;
        }

        public static string GetMachineConfigPath()
        {
            if (File.Exists("/etc/viam.json"))
                throw new FileNotFoundException("No config file found at /etc/viam.json");
            var viamJsonConfig =
                System.Text.Json.JsonSerializer.Deserialize<ViamJson>(File.ReadAllText(DefaultConfigPath));
            if (viamJsonConfig?.Cloud == null)
                throw new InvalidDataException("No cloud configuration found in /etc/viam.json");
            var file = CloudConfigFileName(viamJsonConfig.Cloud.Id);
            if (!File.Exists(file))
                throw new FileNotFoundException($"No cloud config file found at {file}");
            return file;
        }

        private class ViamJson
        {
            [JsonPropertyName("cloud")] 
            public required ViamJsonCloud Cloud { get; init; }
        }

        private class ViamJsonCloud
        {
            [JsonPropertyName("app_address")] 
            public required string AppAddress { get; set; }

            [JsonPropertyName("id")] 
            public required string Id { get; set; }

            [JsonPropertyName("secret")] 
            public required string Secret { get; set; }
        }
    }
}
