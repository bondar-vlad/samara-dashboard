using ChildRights.BuildingBlocks.Application.Abstractions;
using ChildRights.BuildingBlocks.Domain.SharedKernel;
using ChildRights.Analysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChildRights.Analysis.Infrastructure.Persistence;

/// <summary>
/// Seeds the fixed catalogue of admission directions (напрями вступу) with their НМТ
/// competition coefficients and the specialties (спеціальності) grouped under each one
/// (1 direction → many specialties). Powers the admission ("second analysis") widgets.
/// Coefficients are illustrative but reflect the real subject weighting per branch.
/// </summary>
internal sealed class AdmissionCatalogSeeder(AnalysisDbContext context) : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await context.AdmissionDirections.AnyAsync(cancellationToken))
        {
            return;
        }

        // ІТ — галузь знань 12 «Інформаційні технології».
        var it = new AdmissionDirection(
            Guid.NewGuid(), "12", "Інформаційні технології", "12 Інформаційні технології",
            ProfileCluster.ProfessionalInformationTechnology,
            new Dictionary<NmtSubject, double>
            {
                [NmtSubject.Mathematics] = 0.5,
                [NmtSubject.UkrainianLanguage] = 0.2,
                [NmtSubject.HistoryOfUkraine] = 0.1,
                [NmtSubject.Physics] = 0.2
            },
            ["Математика", "Інформатика", "Фізика"],
            ["Програмування", "Алгоритми", "Бази даних"]);
        it.AddSpecialty("121", "Інженерія програмного забезпечення");
        it.AddSpecialty("122", "Комп'ютерні науки");
        it.AddSpecialty("123", "Комп'ютерна інженерія");
        it.AddSpecialty("126", "Інформаційні системи та технології");

        // Право — галузь знань 08 «Право».
        var law = new AdmissionDirection(
            Guid.NewGuid(), "08", "Право", "08 Право",
            ProfileCluster.AcademicSocialHumanitarian,
            new Dictionary<NmtSubject, double>
            {
                [NmtSubject.UkrainianLanguage] = 0.35,
                [NmtSubject.HistoryOfUkraine] = 0.4,
                [NmtSubject.Mathematics] = 0.1,
                [NmtSubject.ForeignLanguage] = 0.15
            },
            ["Правознавство", "Історія", "Українська мова"],
            ["Конституційне право", "Кримінальне право"]);
        law.AddSpecialty("081", "Право");
        law.AddSpecialty("293", "Міжнародне право");

        // Бізнес/економіка — галузь знань 07 «Управління та адміністрування».
        var business = new AdmissionDirection(
            Guid.NewGuid(), "07", "Управління та адміністрування", "07 Управління та адміністрування",
            ProfileCluster.ProfessionalBusinessServices,
            new Dictionary<NmtSubject, double>
            {
                [NmtSubject.Mathematics] = 0.4,
                [NmtSubject.UkrainianLanguage] = 0.25,
                [NmtSubject.HistoryOfUkraine] = 0.15,
                [NmtSubject.ForeignLanguage] = 0.2
            },
            ["Економіка", "Математика", "Англійська мова"],
            ["Фінанси та інвестиції", "Мікроекономіка", "Фінансове право"]);
        business.AddSpecialty("051", "Економіка");
        business.AddSpecialty("072", "Фінанси, банківська справа та страхування");
        business.AddSpecialty("073", "Менеджмент");
        business.AddSpecialty("076", "Підприємництво та торгівля");

        // Медицина — галузь знань 22 «Охорона здоров'я».
        var medicine = new AdmissionDirection(
            Guid.NewGuid(), "22", "Охорона здоров'я", "22 Охорона здоров'я",
            ProfileCluster.ProfessionalLifeSciences,
            new Dictionary<NmtSubject, double>
            {
                [NmtSubject.Biology] = 0.4,
                [NmtSubject.Chemistry] = 0.3,
                [NmtSubject.UkrainianLanguage] = 0.2,
                [NmtSubject.Mathematics] = 0.1
            },
            ["Біологія", "Хімія"],
            ["Анатомія людини", "Генетика", "Органічна хімія"]);
        medicine.AddSpecialty("222", "Медицина");
        medicine.AddSpecialty("226", "Фармація, промислова фармація");
        medicine.AddSpecialty("228", "Педіатрія");

        context.AdmissionDirections.AddRange(it, law, business, medicine);
        await context.SaveChangesAsync(cancellationToken);
    }
}
