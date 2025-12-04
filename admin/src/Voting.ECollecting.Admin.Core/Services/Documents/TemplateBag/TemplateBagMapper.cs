// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.Documents.TemplateBag;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TemplateBagMapper
{
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.Collection), nameof(SignatureSheetAttestationTemplateData.Collection.Address), nameof(SignatureSheetAttestationTemplateData.Collection.Address.CommitteeOrPerson)], nameof(SignatureSheetAttestationTemplateBag.CommitteeName))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.Collection), nameof(SignatureSheetAttestationTemplateData.Collection.Address), nameof(SignatureSheetAttestationTemplateData.Collection.Address.StreetOrPostOfficeBox)], nameof(SignatureSheetAttestationTemplateBag.CommitteeStreet))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.Collection), nameof(SignatureSheetAttestationTemplateData.Collection.Address), nameof(SignatureSheetAttestationTemplateData.Collection.Address.ZipCode)], nameof(SignatureSheetAttestationTemplateBag.CommitteeZipCode))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.Collection), nameof(SignatureSheetAttestationTemplateData.Collection.Address), nameof(SignatureSheetAttestationTemplateData.Collection.Address.Locality)], nameof(SignatureSheetAttestationTemplateBag.CommitteeLocality))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.Collection), nameof(SignatureSheetAttestationTemplateData.Collection.DomainOfInfluenceType)], nameof(SignatureSheetAttestationTemplateBag.DomainOfInfluenceType))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.AclDomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.AclDomainOfInfluence.Name)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityName))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.AclDomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.AclDomainOfInfluence.NameForProtocol)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityNameIncludingType))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Logo), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Logo.Content)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityLogo))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Name)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityAddressName))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Street)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityStreet))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.ZipCode)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityZipCode))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Locality)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityLocality))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Phone)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityPhoneNumber))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Email)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityEmail))]
    [MapProperty([nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence), nameof(SignatureSheetAttestationTemplateData.DomainOfInfluence.Webpage)], nameof(SignatureSheetAttestationTemplateBag.MunicipalityWebsite))]
    public static partial SignatureSheetAttestationTemplateBag MapToSignatureSheetAttestationTemplateBag(SignatureSheetAttestationTemplateData data);

    private static string? MapByteArrayToBase64String(FileContentEntity? file)
    {
        return file == null ? null : Convert.ToBase64String(file.Data);
    }

    private static string MapDate(DateTime date)
    {
        return date.ToString("o", CultureInfo.InvariantCulture);
    }
}
