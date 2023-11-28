buf:
	rm -rf src/Viam.Net.Sdk.Core/Proto
	buf generate buf.build/viamrobotics/api --path common,component,service,robot
	buf generate buf.build/viamrobotics/goutils

run_core_client:
	dotnet run --project examples/Simple/Client http://localhost:8080

run_core_client_web:
	dotnet run --project examples/SimpleWeb/Client http://localhost:8080

run_core_client_auth:
	dotnet run --project examples/SimpleAuth/Client http://localhost:8080

run_core_client_webrtc_auth:
	dotnet run --project examples/SimpleWebRTCAuth/Client http://localhost:8080
