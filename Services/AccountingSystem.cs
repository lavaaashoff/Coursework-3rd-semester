using CouseWork3Semester.Interfaces;
using CouseWork3Semester.Models;
using CouseWork3Semester.Registries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouseWork3Semester.Services
{
    public class AccountingSystem : IAccountingSystem
    {
        public IDormitoryRegistry DormitoryRegistry { get; }
        public IOccupantRegistry OccupantRegistry { get; }
        public ISettlementEvictionService SettlementEvictionService { get; }
        public IPaymentService PaymentService { get; }
        public IReportService ReportService { get; }
        public ISearchService SearchService { get; }
        public IEmployee CurrentEmployee { get; private set; }
        public IAuthManager AuthManager { get; }
        public IPermissionManager PermissionManager { get; }
        public IDocumentOccupantService DocumentOccupantService { get; }
        public IPassportValidator PassportValidator { get; }
        public IDocumentValidator DocumentValidator { get; }
        public IDocumentRegistry DocumentRegistry { get; set; }
        // ДОБАВЛЕНО: реестр инвентаря
        public IInventoryRegistry InventoryRegistry { get; private set; }

        // Базовый конструктор (без currentEmployee), требует inventoryRegistry
        public AccountingSystem(
            IDormitoryRegistry dormitoryRegistry,
            IOccupantRegistry occupantRegistry,
            ISettlementEvictionService settlementEvictionService,
            IPaymentService paymentService,
            IReportService reportService,
            ISearchService searchService,
            IAuthManager authManager,
            IPermissionManager permissionManager,
            IDocumentOccupantService documentOccupantService,
            IPassportValidator passportValidator,
            IDocumentValidator documentValidator,
            IDocumentRegistry documentRegistry,
            IInventoryRegistry inventoryRegistry
        )
        {
            DormitoryRegistry = dormitoryRegistry ?? throw new ArgumentNullException(nameof(dormitoryRegistry));
            OccupantRegistry = occupantRegistry ?? throw new ArgumentNullException(nameof(occupantRegistry));
            SettlementEvictionService = settlementEvictionService ?? throw new ArgumentNullException(nameof(settlementEvictionService));
            PaymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            ReportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            SearchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            AuthManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            PermissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
            DocumentOccupantService = documentOccupantService ?? throw new ArgumentNullException(nameof(documentOccupantService));
            PassportValidator = passportValidator ?? throw new ArgumentNullException(nameof(passportValidator));
            DocumentValidator = documentValidator ?? throw new ArgumentNullException(nameof(documentValidator));
            DocumentRegistry = documentRegistry ?? throw new ArgumentNullException(nameof(documentRegistry));
            InventoryRegistry = inventoryRegistry ?? new InventoryRegistry();

            CurrentEmployee = null;
        }

        // Перегруженный конструктор — как в твоём вызове: с currentEmployee и далее documentRegistry, inventoryRegistry
        public AccountingSystem(
            IDormitoryRegistry dormitoryRegistry,
            IOccupantRegistry occupantRegistry,
            ISettlementEvictionService settlementEvictionService,
            IPaymentService paymentService,
            IReportService reportService,
            ISearchService searchService,
            IAuthManager authManager,
            IPermissionManager permissionManager,
            IDocumentOccupantService documentOccupantService,
            IPassportValidator passportValidator,
            IDocumentValidator documentValidator,
            IEmployee currentEmployee,
            IDocumentRegistry documentRegistry,
            IInventoryRegistry inventoryRegistry
        )
            : this(
                dormitoryRegistry,
                occupantRegistry,
                settlementEvictionService,
                paymentService,
                reportService,
                searchService,
                authManager,
                permissionManager,
                documentOccupantService,
                passportValidator,
                documentValidator,
                documentRegistry,
                inventoryRegistry
            )
        {
            CurrentEmployee = currentEmployee;
        }

        public IEmployee GetCurrentEmployee() => CurrentEmployee;



        // Методы из UML
        public void RegisterOccupant(IRoomOccupant occupant, IRoom room, IDocument document)
        {
            if (occupant == null) throw new ArgumentNullException(nameof(occupant));
            if (room == null) throw new ArgumentNullException(nameof(room));
            if (document == null) throw new ArgumentNullException(nameof(document));

            // Проверка прав (только через интерфейсы)
            if (CurrentEmployee != null && PermissionManager != null)
            {
                if (!PermissionManager.CanRolePerformAction(CurrentEmployee.Role, "ManageOccupants"))
                    throw new InvalidOperationException("Недостаточно прав для выполнения заселения.");
            }

            // Валидация документа (только интерфейс валидатора)
            var format = DocumentValidator.CheckFormat(document);
            if (!format.IsValid)
                throw new InvalidOperationException($"Документ некорректен: {format.Message}");

            if (!DocumentValidator.CheckValidity(document, DateTime.Now))
                throw new InvalidOperationException("Документ недействителен на текущую дату.");

            // Проверка наличия мест в комнате
            if (!room.CheckAvailablePlaces())
                throw new InvalidOperationException($"В комнате №{room.Number} нет свободных мест.");

            // Привязка документа к жильцу (интерфейс сервиса)
            DocumentOccupantService.AttachDocumentToOccupant(document, occupant);

            // Добавление жильца в комнату и реестр (интерфейсы)
            if (!room.AddOccupant(occupant))
                throw new InvalidOperationException("Не удалось добавить жильца в комнату.");

            OccupantRegistry.AddOccupant(occupant);

            // При необходимости можно инициировать платеж (интерфейс IPaymentService),
            // но без конкретных моделей делать это опционально:
            // PaymentService.ChargePayment(occupant, DateTime.Now, amount);
        }

        public void EvictOccupant(IRoomOccupant occupant, string reason)
        {
            if (occupant == null) throw new ArgumentNullException(nameof(occupant));
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Причина выселения должна быть указана.", nameof(reason));

            // Проверка прав (только интерфейсы)
            if (CurrentEmployee != null && PermissionManager != null)
            {
                if (!PermissionManager.CanRolePerformAction(CurrentEmployee.Role, "ManageOccupants"))
                    throw new InvalidOperationException("Недостаточно прав для выполнения выселения.");
            }

            // Находим комнату через интерфейсы общежитий/комнат (без конкретных классов)
            IRoom roomFound = null;
            foreach (var dorm in DormitoryRegistry.GetAllDormitories())
            {
                foreach (var room in dorm.GetAllRooms())
                {
                    if (room.GetAllOccupants().Any(o => o.Id == occupant.Id))
                    {
                        roomFound = room;
                        break;
                    }
                }
                if (roomFound != null) break;
            }

            // Если комнату нашли — удаляем из неё
            if (roomFound != null)
            {
                if (!roomFound.RemoveOccupant(occupant.Id))
                    throw new InvalidOperationException("Не удалось выселить жильца из комнаты.");
            }

            // Удаляем из реестра жильцов
            OccupantRegistry.RemoveOccupant(occupant.Id);

            // При желании можно задействовать сервис выселений через его интерфейс,
            // но без создания конкретной IEviction/ ISettlement здесь ограничимся базовой процедурой.
        }

        public string GetReport(string reportType, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportType))
                throw new ArgumentException("Тип отчёта должен быть указан.", nameof(reportType));

            switch (reportType.Trim().ToLowerInvariant())
            {
                case "dormitory":
                case "общежитие":
                    return ReportService.GenerateDormitoryReport();

                case "settlement":
                case "заселение":
                    if (parameters != null &&
                        parameters.TryGetValue("startDate", out var sdObj) &&
                        parameters.TryGetValue("endDate", out var edObj) &&
                        sdObj is DateTime start && edObj is DateTime end)
                    {
                        return ReportService.GenerateSettlementReport(start, end);
                    }
                    throw new ArgumentException("Для отчёта 'settlement' требуются параметры 'startDate' и 'endDate'.");

                case "free-rooms":
                case "свободные-комнаты":
                    IDormitory dorm = null;
                    if (parameters != null &&
                        parameters.TryGetValue("dormitoryNumber", out var dnObj) &&
                        dnObj is int dormNumber && dormNumber > 0)
                    {
                        dorm = DormitoryRegistry.GetDormitoryByNumber(dormNumber);
                    }
                    return ReportService.GenerateFreeRoomsReport(dorm);

                case "residents-in-dorm":
                case "проживающие":
                    if (parameters != null &&
                        parameters.TryGetValue("dormitoryNumber", out var dnObj2) &&
                        dnObj2 is int dormNumber2 && dormNumber2 > 0)
                    {
                        return ReportService.GenerateDormResidentsReport(dormNumber2);
                    }
                    throw new ArgumentException("Для отчёта 'residents-in-dorm' требуется параметр 'dormitoryNumber'.");

                default:
                    throw new NotSupportedException($"Неизвестный тип отчёта: {reportType}");
            }
        }

        // Доп. методы из интерфейса
        public object Search(string searchType, string query)
        {
            if (string.IsNullOrWhiteSpace(searchType))
                throw new ArgumentException("Тип поиска должен быть указан.", nameof(searchType));

            switch (searchType.Trim().ToLowerInvariant())
            {
                case "occupant":
                case "житель":
                    return SearchService.FindOccupant(query ?? string.Empty);

                case "dormitory":
                case "общежитие":
                    return SearchService.FindDormitoryByAddress(query ?? string.Empty);

                case "rooms":
                case "комнаты":
                    // Ожидаем, что query = номер общежития
                    if (int.TryParse(query, out var dormNumber))
                    {
                        var dormitory = DormitoryRegistry.GetDormitoryByNumber(dormNumber);
                        if (dormitory == null) return new List<IRoom>();
                        return SearchService.FindAvailableRooms(dormitory);
                    }
                    return new List<IRoom>();

                default:
                    return null;
            }
        }
        // Вспомогательный метод: обновить текущего сотрудника (через интерфейс IEmployee)
        public void SetCurrentEmployee(IEmployee employee)
        {
            CurrentEmployee = employee;
        }
    }
}