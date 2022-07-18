buf:
	rm -rf gen
	buf generate buf.build/viamrobotics/rdk
	buf generate buf.build/viamrobotics/goutils
	buf generate buf.build/erdaniels/gostream

run_core_client:
	dotnet run --project examples/Simple/Client http://localhost:8080

run_core_client_web:
	dotnet run --project examples/SimpleWeb/Client http://localhost:8080

run_core_client_auth:
	dotnet run --project examples/SimpleAuth/Client http://localhost:8080

run_core_client_webrtc_auth:
	dotnet run --project examples/SimpleWebRTCAuth/Client http://localhost:8080

prepare_unity_app:
	dotnet build src/Viam.Net.Sdk.Core
	cp src/Viam.Net.Sdk.Core/bin/Debug/netstandard2.1/Viam.Net.Sdk.Core.dll examples/UnityApp/Assets/
