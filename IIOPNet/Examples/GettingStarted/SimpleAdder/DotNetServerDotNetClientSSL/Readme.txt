This demo shows how to communicate using SSL.

The client and the server expects, that the provided certificate are installed in the windows certificate store as following:
- ServerKeyPair.pfx in the personal certificate store of localuser (password is demo)
- serverCertificate.cer in the root certificate store of localuser

- ClientKeyPair.pfx in the personal certificate store of localuser (password is demo)
- clientCertificate.cer in the root certificate store of localuser


This demo does only work under windows, because the ssl library used makes heavy use of the windows crypto library.
