# ✨ Changelog (`v1.138.1`)

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Version Info

```text
This version -------- v1.138.1
Previous version ---- v1.132.13
Initial version ----- v1.125.3
Total commits ------- 21
```

## [v1.138.1] - 2026-03-16

### 🔄 Changed

- fix usage of ArgumentOutOfRangeException for non-arguments

## [v1.138.0] - 2026-03-09

### 🔄 Changed

- feat(VOTING-6818): add doi address name

## [v1.137.0] - 2026-03-09

### 🔄 Changed

- feat(VOTING-6334): rm initiative quorum on federal level

## [v1.136.0] - 2026-03-04

### 🆕 Added

- feat(VOTING-6729): initiative wording as markdown

## [v1.135.2] - 2026-02-27

### 🔄 Changed

- fix(VOTING-6334): set signature counts correctly on dois and init subtype to single type for doi type CH

## [v1.135.1] - 2026-02-25

### 🔄 Changed

- fix(VOTING-6334): inherit mu max electronic quorum from ct on doi list

### 🔄 Changed

- fix(VOTING-6334): include doi logo for attestation

## [v1.135.0] - 2026-02-24

### 🆕 Added

- feat(VOTING-6827): add counts on attestation

## [v1.134.1] - 2026-02-23

### 🔄 Changed

- make phone, email and website on signature sheet attestation optional

## [v1.134.0] - 2026-02-19

### 🆕 Added

- feat(VOTING-6808): add initiatives cleanup job

## [v1.133.7] - 2026-02-18

### 🔄 Changed

- list decree only include referendums from parents only in period state in collection or expired

## [v1.133.6] - 2026-02-18

### 🔄 Changed

- ensure all signature sheets are past attested to add samples

## [v1.133.5] - 2026-02-18

### 🔄 Changed

- reorder metrics middleware calls in Startup configuration to catch final response status.

## [v1.133.4] - 2026-02-16

### ❌ Removed

- fix(VOTING-6368): rm additional notification emails

## [v1.133.3] - 2026-02-13

### 🔄 Changed

- include all collection with keys for is other referendum signed check

## [v1.133.2] - 2026-02-13

### 🔄 Changed

- remove filename generation in UI

## [v1.133.1] - 2026-02-13

### 🔄 Changed

- adjust filenames

## [v1.133.0] - 2026-02-12

### 🔄 Changed

- feat(VOTING-6334): move settings from voting basis and merge domain of influence with acl dois.

## [v1.132.14] - 2026-02-12

### 🔄 Changed

- rename withdrawn state for referendums

## [v1.132.13] - 2026-02-09

### 🆕 Added

- add update committee members political duty

## [v1.132.12] - 2026-02-09

### 🔄 Changed

- split committee members into two tables

## [v1.132.11] - 2026-02-06

### 🔄 Changed

- extend CD pipeline with enhanced bug bounty publication workflow

## [v1.132.10] - 2026-02-05

### 🔄 Changed

- is other referendum signed check filters referendums in preparation

## [v1.132.9] - 2026-02-04

### 🔄 Changed

- can finish and can generate documents only if referendums are present

## [v1.132.8] - 2026-02-03

### 🔄 Changed

- signature sheet received at cannot be in the future

## [v1.132.7] - 2026-01-30

### 🔄 Changed

- list collections includes initiative subtypes

## [v1.132.6] - 2026-01-30

### 🔄 Changed

- update committee member sort does not affect other initiatives

## [v1.132.5] - 2026-01-29

### 🔄 Changed

- list my contains only enabled doi types

## [v1.132.4] - 2026-01-29

### 🔄 Changed

- resend committee member invitation and collection permission

## [v1.132.3] - 2026-01-27

### 🆕 Added

- fix(VOTING-6673): upgrade lib with fixed sc token injection

## [v1.132.2] - 2026-01-26

### 🔄 Changed

- limit access for municipalities on ct collections

## [v1.132.1] - 2026-01-21

### 🔄 Changed

- fix(VOTING-6672): relax government decision number validation

## [v1.132.0] - 2026-01-21

### 🆕 Added

- Draft: feat(VOTING-6671): add secure number to collections

## [v1.131.2] - 2026-01-20

### 🔄 Changed

- fix(VOTING-6366): fix collectionpermissions iamuserid index

## [v1.131.1] - 2026-01-20

### 🔄 Changed

- add collection signature type

## [v1.131.0] - 2026-01-20

### 🆕 Added

- feat(VOTING-6366): collection owner permission

## [v1.130.0] - 2026-01-19

### 🆕 Added

- add support for canton AR during ACL import

## [v1.129.7] - 2026-01-19

### 🔄 Changed

- allow reset committee member for iam signature type

## [v1.129.6] - 2026-01-19

### 🔄 Changed

- fix(VOTING-6650): government decision number unique should be case-insensitive

## [v1.129.5] - 2026-01-19

### 🔄 Changed

- open chat for mail links

## [v1.129.4] - 2026-01-15

### 🆕 Added

- fix(VOTING-6650): unique gov decision numbers

## [v1.129.3] - 2026-01-15

### 🔄 Changed

- adjust logo deleted text

## [v1.129.2] - 2026-01-15

### 🆕 Added

- fix(VOTING-6752): validate link urls

## [v1.129.1] - 2026-01-15

### 🔄 Changed

- check for enabled doi type for setting a collection in preparation

## [v1.129.0] - 2026-01-15

### 🆕 Added

- add street, house number and zip code for committee member

## [v1.128.0] - 2026-01-14

### 🆕 Added

- add statistical data time lapse export

## [v1.127.6] - 2026-01-14

### 🆕 Added

- add modified by superior authority flag for signature sheets

## [v1.127.5] - 2026-01-12

### 🔄 Changed

- load domain of influence name after create with admissibility

## [v1.127.4] - 2026-01-12

### 🔄 Changed

- fix(VOTING-6670): default locked fields for new admissibility decisions

## [v1.127.3] - 2026-01-12

### 🔄 Changed

- move statistical data and official journal publication to decree

## [v1.127.2] - 2026-01-09

### 🆕 Added

- add can download committee list user permission

## [v1.127.1] - 2026-01-09

### 🔄 Changed

- fix(VOTING-6546): respect user-entered newlines in accessibility email

## [v1.127.0] - 2026-01-09

### 🔄 Changed

- feat(VOTING-6551): add expired approval states

## [v1.126.0] - 2026-01-08

### 🔄 Changed

- feat(VOTING-6502): increased max length on referendum committee field

## [v1.125.9] - 2026-01-08

### 🔄 Changed

- detach political residence from official residence

## [v1.125.8] - 2026-01-07

### 🆕 Added

- add domain of influence name for decree and admissibility decisions

## [v1.125.7] - 2026-01-07

### 🔄 Changed

- change collection start and end date to date only

## [v1.125.6] - 2026-01-07

### 🔄 Changed

- remove approved committee members max valid validation for Mu and Ct

## [v1.125.5] - 2025-12-19

### 🔄 Changed

- fix user permission can finish correction

## [v1.125.4] - 2025-12-18

### 🔄 Changed

- change seeder texts

## [v1.125.3] - 2025-12-04

### 🎉 Initial release for Bug Bounty
