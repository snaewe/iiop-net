@echo 01 > serial
@echo "create an empty file index.txt, if no file exists"
@if not exist index.txt echo creating new index.txt
@if not exist index.txt copy index.txt.begin index.txt

@if not exist newcerts echo "creating newcerts directory"
@if not exist newcerts mkdir newcerts

@echo creating CA-certificate

openssl.exe req -new -x509 -extensions v3_ca -keyout cakey.pem -out cacert.pem -days 3650 -config .\openssl.conf

@echo creating server certificate signing request

openssl req -new -nodes -out serverreq.pem -keyout serverkey.pem -config .\openssl.conf

@echo signing server certificate req

openssl ca -extensions server_ext -extfile serverExtensions.txt -out servercert.pem -config .\openssl.conf -infiles serverreq.pem

@echo creating client certificate signing request

openssl req -new -nodes -out clientreq.pem -keyout clientkey.pem -config .\openssl.conf

@echo signing client certificate req

openssl ca -extensions client_ext -extfile clientExtensions.txt -out clientcert.pem -config .\openssl.conf -infiles clientreq.pem


@echo creating certificates to import in private keystore

openssl pkcs12 -export -out server\server.p12 -in servercert.pem -inkey serverkey.pem
openssl pkcs12 -export -out client\client.p12 -in clientcert.pem -inkey clientkey.pem

@echo creating der-encoded ca-cert for import into root keystore (only public key included)
openssl x509 -in cacert.pem -outform DER -out cacert.cer


