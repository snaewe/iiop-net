This demo shows how to communicate using SSL.

The client and the server expects, that the provided certificate are installed in the windows certificate store as following:
- server\server.p12 in the personal certificate store of localuser (password is demo)
- client\client.p12 in the personal certificate store of localuser (password is demo)
- cacert.cer in the root certificate store of localuser
HINT: New certificates may be generated using the provided makeCACert.bat. (needs OpenSSL)

Afterwards, the server can be started using startServer.bat and the client can be started using runClient.bat.

NOTE:
This demo does only work under windows, because the ssl library used makes heavy use of the windows crypto library.
