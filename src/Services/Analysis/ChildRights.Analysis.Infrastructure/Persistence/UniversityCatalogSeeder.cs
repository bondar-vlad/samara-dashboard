using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Infrastructure.Persistence;

/// <summary>
/// Seeds the fixed catalogue of universities and their programmes (спеціальності), mapped to
/// reform clusters with the key subjects/topics that drive student-fit and gap analysis.
/// </summary>
internal sealed class UniversityCatalogSeeder(AnalysisDbContext context) : IDataSeeder
{
    private static readonly Guid KpiId = Guid.Parse("aaaa1111-0000-0000-0000-000000000001");
    private static readonly Guid KnuId = Guid.Parse("aaaa1111-0000-0000-0000-000000000002");
    private static readonly Guid NmuId = Guid.Parse("aaaa1111-0000-0000-0000-000000000003");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Universities.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Universities.AddRange(
            new University(KpiId, "КПІ ім. Ігоря Сікорського", "Київ", "Київська область"),
            new University(KnuId, "Київський національний університет ім. Тараса Шевченка", "Київ", "Київська область"),
            new University(NmuId, "Національний медичний університет ім. О. О. Богомольця", "Київ", "Київська область"));

        context.UniversityPrograms.AddRange(
            Program(KpiId, "КПІ ім. Ігоря Сікорського", "Комп'ютерні науки",
                ProfileCluster.ProfessionalInformationTechnology,
                [EducationProfile.InformationTechnology, EducationProfile.EngineeringTechnological],
                ["Математика", "Інформатика"], ["Програмування", "Алгоритми"], 9.5),

            Program(KpiId, "КПІ ім. Ігоря Сікорського", "Інженерія програмного забезпечення",
                ProfileCluster.ProfessionalInformationTechnology,
                [EducationProfile.InformationTechnology],
                ["Математика", "Інформатика"], ["Програмування", "Бази даних"], 9.0),

            Program(KpiId, "КПІ ім. Ігоря Сікорського", "Будівництво та цивільна інженерія",
                ProfileCluster.ProfessionalTechnical,
                [EducationProfile.Construction, EducationProfile.EngineeringTechnological],
                ["Математика", "Фізика"], ["Креслення", "Будівельні конструкції"], 8.0),

            Program(KnuId, "КНУ ім. Тараса Шевченка", "Право",
                ProfileCluster.AcademicSocialHumanitarian,
                [EducationProfile.SocialHumanitarian, EducationProfile.EducationalHumanitarian],
                ["Правознавство", "Історія"], ["Конституційне право", "Кримінальне право"], 9.0),

            Program(KnuId, "КНУ ім. Тараса Шевченка", "Фінанси, банківська справа та страхування",
                ProfileCluster.ProfessionalBusinessServices,
                [EducationProfile.BusinessAdministration],
                ["Математика", "Економіка"], ["Фінанси та інвестиції", "Фінансове право"], 9.0),

            Program(KnuId, "КНУ ім. Тараса Шевченка", "Менеджмент",
                ProfileCluster.ProfessionalBusinessServices,
                [EducationProfile.BusinessAdministration, EducationProfile.HospitalityEvents],
                ["Економіка", "Англійська мова"], ["Менеджмент подій", "Маркетинг"], 8.0),

            Program(NmuId, "НМУ ім. О. О. Богомольця", "Медицина",
                ProfileCluster.ProfessionalLifeSciences,
                [EducationProfile.Medical],
                ["Біологія", "Хімія"], ["Анатомія людини", "Генетика"], 10.0),

            Program(NmuId, "НМУ ім. О. О. Богомольця", "Фармація",
                ProfileCluster.ProfessionalLifeSciences,
                [EducationProfile.Medical, EducationProfile.Agricultural],
                ["Хімія", "Біологія"], ["Органічна хімія"], 9.0));

        await context.SaveChangesAsync(cancellationToken);
    }

    private static UniversityProgram Program(
        Guid universityId,
        string universityName,
        string name,
        ProfileCluster cluster,
        EducationProfile[] profiles,
        string[] subjects,
        string[] topics,
        double minAverage) =>
        new(Guid.NewGuid(), universityId, universityName, name, cluster, profiles, subjects, topics, minAverage);
}
