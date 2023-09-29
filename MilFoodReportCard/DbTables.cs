using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MilFoodReportCard
{
    internal class DbTables
    {
    }

    [Table("спрВЧ")]
    public class Division
    {
        [Key]
        [Column("Наименование")]
        public string DivisionName { get; set; }
    }

    [Table("спрРаскладки")]
    public class Layout
    {
        [Key]
        [Column("Код")]
        public int LayoutId { get; set; }
        [Column("ВЧ")]
        public String Division { get; set; }
        [Column("Год")]
        public int Year { get; set; }
        [Column("Период")]
        public int Period { get; set; }
        [Column("Неделя")]
        public int Week { get; set; }
        [Column("ФайлРаскладки")]
        public string LayoutFile { get; set; }
        [Column("ТипПитания")]
        public string NutritionKind { get; set; }
    }

    [Table("спрСтроевкаПитание")]
    public class FedLayout
    {
        [Key]
        [Column("Код")]
        public int FedLayoutId { get; set; }
        [Column("Раскладка")]
        public int LayoutId { get; set; }
        public virtual Layout Layout { get; set; }
        [Column("Дата")]
        public DateTime Date { get; set; }
        [Column("ПриемЕды")]
        public int MealId { get; set; }
        public virtual Meal Meal { get; set; }
        [Column("КоличествоЧеловек")]
        public double FedNumber { get; set; }
        [Column("ПланПроцент")]
        public float Weight { get; set; }
        [Column("Договор")]
        public int? AgreementId { get; set; }
        public virtual Agreement Agreement { get; set; }
        [Column("ВведеноВпрограмме")]
        public bool? IsSetProgrammatically { get; set; }
        //----
        //public virtual ICollection<ДокРасходСтроевкаТч> ДокРасходСтроевкаТчs { get; } = new List<ДокРасходСтроевкаТч>();
    }

    [Table("спрПриемыЕды")]
    public class Meal
    {
        [Key]
        [Column("Код")]
        public int MealId { get; set; }
        [Column("Наименование")]
        public string Name { get; set; }
        [Column("Краткое наименование")]
        public string ShortName { get; set; }
    }

    [Table("спрПродукт")]
    public class Product
    {
        [Key]
        [Column("Артикул")]
        public string ProductId { get; set; }
        [Column("Наименование")]
        public string Name { get; set; }
        [Column("ЕдИзмОсн")]
        public string MainUoM { get; set; }
        [Column("ЕдИзмУчет")]
        public string AccUoM { get; set; }
        [Column("ЕдИзмКоэф")]
        public int ScaleUoM { get; set; }
        [Column("Калор")]
        public int? Calories { get; set; }
        [Column("Группа")]
        public string? Group { get; set; }
        [Column("НормаКоЭФ")]
        public float? Norm { get; set; }

        //public virtual СпрГруппаТоваров? ГруппаNavigation { get; set; }
        //public virtual ICollection<ДокПеремещениеТч> ДокПеремещениеТчs { get; } = new List<ДокПеремещениеТч>();
        //public virtual ICollection<ДокПриходТч> ДокПриходТчs { get; } = new List<ДокПриходТч>();
        //public virtual ICollection<ДокРасходТч> ДокРасходТчs { get; } = new List<ДокРасходТч>();
        //public virtual ICollection<СпрСоставРаскладки> СпрСоставРаскладкиs { get; } = new List<СпрСоставРаскладки>();
        //public virtual ICollection<СпрСпецификацияДоговора> СпрСпецификацияДоговораs { get; } = new List<СпрСпецификацияДоговора>();
    }

    //public class Remain
    //{
    //    [Key]
    //    [Column("Артикул")]
    //    public string ProductId { get; set; }
    //    [Column("Наименование")]
    //    public string ProductName { get; set; }
    //    [Column("ЕдИзмУчет")]
    //    public string AccUoM { get; set; }
    //    [Column("Кво")]
    //    public float Quantity { get; set; }
    //    [Column("Сум")]
    //    public float Cost { get; set; }
    //}

    [Table("докРасход")]
    public class OutgoingsDoc
    {
        [Key]
        [Column("Код")]
        public int OutgoingsDocId { get; set; }
        [Column("ВЧ")]
        public string Division { get; set; }
        [Column("Склад")]
        public int WarehouseId { get; set; }
        [Column("ДатаДок")]
        public DateTime DocDate { get; set; }
        [Column("НомерДок")]
        public string Number { get; set; }
        [Column("ВидОбеспечения")]
        public string ProvisionKind { get; set; }
        [Column("ДатаРаскладки")]
        public DateTime LayoutDate { get; set; }
        [Column("Договор")]
        public int AgreementId { get; set; }
        public virtual Agreement Agreement { get; set; }
        [Column("Примечание")]
        public string Summary { get; set; }
        [Column("Тип списания")]
        public string WritingOffType { get; set; }
        [Column("Раскладка")]
        public int? LayoutId { get; set; }
        public virtual Layout? Layout { get; set; }
        [Column("Перемещение")]
        public int? MovementId { get; set; }
        //public virtual Movement Movement { get; set; }
        [Column("Акт")]
        public int? ActId { get; set; }
        //public virtual Act Act { get; set; }
        [Column("ПодразделениеВЧ")]
        public string? Subdivision { get; set; }
        [Column("РаскладкаПодр")]
        public int? SubdivisionLayoutId { get; set; }
        public virtual Layout? SubdivisionLayout { get; set; }
        [Column("ДатаСПодр")]
        public DateTime? SubdivisionFromDate { get; set; }
        [Column("ДатаПоПодр")]
        public DateTime? SubdivisionToDate { get; set; }
        [Column("ОтправленВОтчетеДата")]
        public DateTime? SentReportDate { get; set; }
        [Column("ПолученОтчетДата")]
        public DateTime? ReceivedReportDate { get; set; }
        [Column("КоррДок")]
        public int? CorrDoc { get; set; }
        [Column("СуммаСписана")]
        public double? WritingOffSum { get; set; }
        [Column("СуммаВозможная")]
        public double? PossibleSum { get; set; }
        [Column("СуммаЭкономия")]
        public double? EconomySum { get; set; }
        [Column("СтатусФайла")]
        public bool? FileState { get; set; }
        [Column("КодДокФайла")]
        public double? DocFileCode { get; set; }

        [Column("ОтправленоФайл")]
        public DateTime? SentFile { get; set; }
        [Column("ПолученоФайл")]
        public DateTime? ReceivedFile { get; set; }
        [Column("ОтветФайла")]
        public DateTime? ResponseFile { get; set; }
        [Column("ЗалишПоДок")]
        public string? DocumentRemains { get; set; }
        [Column("ПоЧеку")]
        public string? CheckNumber { get; set; }
        [Column("ВчЧек")]
        public string? CheckDivision { get; set; }
        [Column("РаспоряжениеЧек")]
        public string? CheckOrder { get; set; }
        [Column("КодДокПриход")]
        public int? IncomesDocumentCode { get; set; }
        [Column("ТипКЕКВ")]
        public string? CatalogType { get; set; }
        [Column("Период")]
        public int? ProdPeriodPeriod { get; set; }
        [Column("Неделя")]
        public int? ProdPeriodWeek { get; set; }
        [Column("Год")]
        public int? ProdPeriodYear { get; set; }
        [Column("СтанПДВ")]
        public bool? VatState { get; set; }
    }

    [Table("докРасходТЧ")]
    public class OutgoingsDoc1
    {
        [Key]
        [Column("Код")]
        public int OutgoingsDoc1Id { get; set; }
        [Column("Док")]
        public int OutgoingsDocId { get; set; }
        public virtual OutgoingsDoc OutgoingsDoc { get; set; }
        [Column("Продукт")]
        public string? ProductId { get; set; }
        public virtual Product? Product { get; set; }
        [Column("Количество")]
        public double Amount { get; set; }
        [Column("Сумма")]
        public double Sum { get; set; }
        [Column("ПриемЕды")]
        public int? MealId { get; set; }
        public virtual Meal? Meal { get; set; }
        [Column("Цена")]
        public double? Price { get; set; }
    }

    [Table("докПриходОтПоставщика")]
    public class IncomesDoc
    {
        [Key]
        [Column("Код")]
        public int IncomesDocId { get; set; }
        [Column("ВЧ")]
        public string Division { get; set; }
        [Column("Склад")]
        public int WarehouseId { get; set; }
        [Column("ДатаДок")]
        public DateTime DocDate { get; set; }
        [Column("НомерДок")]
        public string? Number { get; set; }
        [Column("Договор")]
        public int AgreementId { get; set; }
        public virtual Agreement Agreement { get; set; }
        [Column("номерПоставщика")]
        public string? SupplierNumber { get; set; }
        [Column("ДатаПоставщика")]
        public DateTime? SupplierDate { get; set; }
        [Column("АвтоМарка")]
        public string? VehicleModel { get; set; }
        [Column("АвтоНомер")]
        public string? VehicleNumber { get; set; }
        [Column("ФИОВодитель")]
        public string? DriverName { get; set; }
        [Column("НомПломбы")]
        public string? SealNumber { get; set; }
        [Column("Примечание")]
        public string? Note { get; set; }
        /*
    public int? Акт { get; set; }
    public int? Перемещение { get; set; }
    public string? ТипПрихода { get; set; }
    public string? ПришлоСвч { get; set; }
    public DateTime? ОтправленВотчетеДата { get; set; }
    public DateTime? ПолученОтчетДата { get; set; }
    public bool? Подрядник { get; set; }
    public string? НомерПодрядника { get; set; }
    public DateTime? ДатаПодрядника { get; set; }
    public string? ТипКекв { get; set; }
    public bool? СтатусФайла { get; set; }
    public decimal? КодДокФайла { get; set; }
    public DateTime? ОтправленоФайл { get; set; }
    public DateTime? ПолученоФайл { get; set; }
    public DateTime? ОтветФайла { get; set; }
    public string? ЗалишПоДок { get; set; }
    public int? КодДокРасход { get; set; }
    public int? Период { get; set; }
    public int? Неделя { get; set; }
    public bool? ЗаказНаПоставку { get; set; }
    public int? Год { get; set; }
    public decimal? АвтоТемпер { get; set; }
    public virtual ДокАктПоставки? АктNavigation { get; set; }
    public virtual СпрВч ВчNavigation { get; set; } = null!;
    public virtual СпрДоговорПоставки? ДоговорNavigation { get; set; }
    public virtual ICollection<ДокАктПоставкиТч> ДокАктПоставкиТчs { get; } = new List<ДокАктПоставкиТч>();
    public virtual ICollection<ДокПеремещение> ДокПеремещениеs { get; } = new List<ДокПеремещение>();
    public virtual ICollection<ДокПриходТч> ДокПриходТчs { get; } = new List<ДокПриходТч>();
    public virtual ICollection<ДокПриходТчКритерий> ДокПриходТчКритерийs { get; } = new List<ДокПриходТчКритерий>();
    public virtual СпрСклад? СкладNavigation { get; set; }
         */
        //[Column("Акт")]
        //[Column("Перемещение")]
        //[Column("ТипПрихода")]
        //[Column("ПришлоСВЧ")]
        //[Column("ОтправленВОтчетеДата")]
        //[Column("ПолученОтчетДата")]
        //[Column("Подрядник")]
        //[Column("НомерПодрядника")]
        //[Column("ДатаПодрядника")]
        //[Column("ТипКЕКВ")]
        //[Column("СтатусФайла")]
        //[Column("КодДокФайла")]
        //[Column("ОтправленоФайл")]
        //[Column("ПолученоФайл")]
        //[Column("ОтветФайла")]
        //[Column("ЗалишПоДок")]
        //[Column("КодДокРасход")]
        //[Column("Период")]
        //[Column("Неделя")]
        //[Column("ЗаказНаПоставку")]
        //[Column("Год")]
        //[Column("АвтоТемпер")]
    }

    [Table("докПриходТЧ")]
    public class IncomesDoc1
    {
        [Key]
        [Column("Код")]
        public int IncomesDoc1Id { get; set; }
        [Column("Док")]
        public int IncomesDocId { get; set; }
        public virtual IncomesDoc IncomesDoc { get; set; }
        [Column("Продукт")]
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Column("Количество")]
        public double Amount { get; set; }
        [Column("Сумма")]
        public double Sum { get; set; }
        [Column("Дата изготовления")]
        public DateTime? ProdDate { get; set; }
        [Column("Дата потребления")]
        public DateTime? ConsDate { get; set; }
        //[Column("Срок")]
        //public virtual ....
        [Column("Цена")]
        public double? Price { get; set; }
        //[Column("КоличествоДок")]
        //[Column("СертНомер")]
        //[Column("СертДата")]
        //[Column("СертКол")]
    }

    [Table("спрСоставРаскладки")]
    public class LayoutEntry
    {
        [Key]
        [Column("Код")]
        public int LayoutEntryId { get; set; }
        [Column("Раскладка")]
        public int LayoutId { get; set; }
        [Column("Дата")]
        public DateTime Date { get; set; }
        [Column("ПриемЕды")]
        public int MealId { get; set; }
        public virtual Meal Meal { get; set; }
        [Column("ВидБлюда")]
        public int CourseKindId { get; set; }
        [Column("Блюдо")]
        public int CourseId { get; set; }
        [Column("КоэфПриготовления")]
        public float Factor { get; set; }
        [Column("Продукт")]
        public string ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Column("Количество")]
        public double Amount { get; set; }
        [Column("ЕдиницаИзмерения")]
        public int UoMId { get; set; }
        [Column("Договор")]
        public int AgreementId { get; set; }
    }

    [Table("спрСпецификацияДоговора")]
    public class AgreementDetails
    {
        [Key]
        [Column("Код")]
        public int AgreementDetailsId { get; set; }

        [Column("Договор")]
        public int AgreementId  { get; set; }
        public virtual Agreement Agreement { get; set; }

        [Column("Товар")]
        public string ProductId  { get; set; }
        public virtual Product Product { get; set; }

        [Column("Количество")]
        public float Amount { get; set; }

        [Column("Цена")]
        public float Price { get; set; }

        [Column("Пакування")]
        public string? Packing { get; set; }

        [Column("Калор")]
        public decimal? Calories { get; set; }
    }


    [Table("спрДоговорПоставки")]
    public class Agreement
    {
        [Key]
        [Column("Код")]
        public int AgreementId { get; set; }

        [Column("НомерДоговора")]
        public string Number { get; set; }

        [Column("ДатаДоговора")]
        public DateTime Date { get; set; }

        [Column("Наименование")]
        public string Name { get; }
        //public virtual string Name()
        //{
        //    return "Дог. № " + Number + " від " + Date.ToShortDateString;
        //}

        [Column("ДатаНачала")]
        public DateTime StartDate { get; set; }

        [Column("ДатаОкончания")]
        public DateTime EndDate { get; set; }

        [Column("Поставщик")]
        public string? Supplier { get; set; }

/*        public int? Служебный { get; set; }

        public string? Директор { get; set; }

        public decimal? Вартість { get; set; }

        public decimal? ЦінаПослО { get; set; }

        public decimal? ЦінаПослС { get; set; }

        public decimal? ЦінаПослОбезПдв { get; set; }

        public bool? СтанСтарогоДог { get; set; }

        public virtual ICollection<ДокАктПоставки> ДокАктПоставкиs { get; } = new List<ДокАктПоставки>();

        public virtual ICollection<ДокАктСписания> ДокАктСписанияs { get; } = new List<ДокАктСписания>();

        public virtual ICollection<ДокАктСписанияПос> ДокАктСписанияПосs { get; } = new List<ДокАктСписанияПос>();

        public virtual ICollection<ДокЗаказПродуктов> ДокЗаказПродуктовs { get; } = new List<ДокЗаказПродуктов>();

        public virtual ICollection<ДокПеремещение> ДокПеремещениеs { get; } = new List<ДокПеремещение>();

        public virtual ICollection<ДокПриходОтПоставщика> ДокПриходОтПоставщикаs { get; } = new List<ДокПриходОтПоставщика>();

        public virtual ICollection<ДокРасход> ДокРасходs { get; } = new List<ДокРасход>();

        public virtual ICollection<СпрСоставРаскладки> СпрСоставРаскладкиs { get; } = new List<СпрСоставРаскладки>();

        public virtual ICollection<СпрСпецификацияДоговора> СпрСпецификацияДоговораs { get; } = new List<СпрСпецификацияДоговора>();

        public virtual ICollection<СпрСтроевкаПитание> СпрСтроевкаПитаниеs { get; } = new List<СпрСтроевкаПитание>();
 */   }

    [Table("докРасходСтроевкаТч")]
    public partial class OutgoingsDocFed
    {
        [Key]
        [Column("Код")]
        public int OutgoingsDocFedId { get; set; }

        [Column("Док")]
        public int OutgoingsDocId { get; set; }
        public virtual OutgoingsDoc OutgoingsDoc { get; set; }

        [Column("Строевка")]
        public int FedLayoutId { get; set; }
        public virtual FedLayout FedLayout { get; set; }
    }

}