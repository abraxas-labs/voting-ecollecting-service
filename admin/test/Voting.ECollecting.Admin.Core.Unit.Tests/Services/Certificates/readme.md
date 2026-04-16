# Certificates

Create a new CA:
```bash
openssl req \
  -new \
  -newkey rsa:4096 \
  -config openssl_ca.cnf -extensions v3_ca \
  -days 1825 \
  -x509 \
  -sha512 -sigopt rsa_padding_mode:pss \
  -passout pass:123456 \
  -subj "/C=CH/ST=St.Gallen/L=St.Gallen/O=Staatskanzlei des Kantons St.Gallen/OU=DfPR/CN=E-Col CA-Zertifikat Test" \
  -keyout ca-certificate.key \
  -out ca-certificate.pem
```

Add new CA to AppConfig `Voting.ECollecting.Admin.Core.Configuration.BackupCertificateConfig.CACertificate` (read cert with `openssl x509 -in ca-certificate.pem -text`)

Create a new certificate:
```bash
openssl req \
  -new \
  -newkey rsa:4096 \
  -config openssl_backup.cnf -extensions v3_req_rsa \
  -days 120 \
  -x509 \
  -CA ca-certificate.pem \
  -CAkey ca-certificate.key \
  -sha512 -sigopt rsa_padding_mode:pss \
  -passin pass:123456 \
  -passout pass:123456 \
  -subj "/C=CH/ST=St.Gallen/L=St.Gallen/O=Staatskanzlei des Kantons St.Gallen/OU=DfPR/CN=E-Col Backup-Zertifikat Test" \
  -keyout certificate.key \
  -out certificate.pem
```
