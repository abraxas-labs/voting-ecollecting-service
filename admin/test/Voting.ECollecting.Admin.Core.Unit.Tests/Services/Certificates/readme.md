# Certificates

Create a new CA:
```bash
openssl req \
  -new \
  -newkey ec \
  -pkeyopt ec_paramgen_curve:secp384r1 \
  -days 1825 \
  -x509 \
  -sha384 \
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
  -newkey ec \
  -pkeyopt ec_paramgen_curve:secp384r1 \
  -days 365 \
  -x509 \
  -CA ca-certificate.pem \
  -CAkey ca-certificate.key \
  -sha384 \
  -passin pass:123456 \
  -passout pass:123456 \
  -subj "/C=CH/ST=St.Gallen/L=St.Gallen/O=Staatskanzlei des Kantons St.Gallen/OU=DfPR/CN=E-Col Backup-Zertifikat Test" \
  -keyout certificate.key \
  -out certificate.pem
```
