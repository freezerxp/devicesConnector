using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;
using devicesConnector.Helpers;
using System.Text.Json;
using static devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices.Russia
{
    // TODO: Проверка на состояние устройства
    // TODO: Обработка действий при ошибке устройства
    // TODO: Захват мира!

    public class PortDriverRu : IFiscalRegistrarDevice
    {
        /// <summary>
        /// Драйвер
        /// </summary>
        private dynamic? _driver;
        /// <summary>
        /// Настройки устройства
        /// </summary>
        private Device _deviceConfig;

        public PortDriverRu(Device device)
        {
            _deviceConfig = device;
        }

        public void Connect()
        {
            _driver = CommonHelper.CreateObject(@"AddIn.PortKKT54FZ");

            if (_driver == null)
                throw new NullReferenceException();
            
            _driver.Init();
            _driver.SaveLog = 1;

            if (_deviceConfig.Connection.ConnectionType == Common.DeviceConnection.ConnectionTypes.ComPort)
            {
                if (_deviceConfig.Connection.ComPort == null)
                    throw new NullReferenceException();
                
                _driver.Connect(_deviceConfig.Connection.ComPort.PortName, _deviceConfig.Connection.ComPort.Speed);
            }
            else
            {
                if (_deviceConfig.Connection.Lan == null)
                    throw new NullReferenceException();
                
                int port = (int)(_deviceConfig.Connection.Lan.PortNumber == null
                    || _deviceConfig.Connection.Lan.PortNumber == 0
                    ? 3999
                    : _deviceConfig.Connection.Lan.PortNumber);

                _driver.ConnectToSocket(_deviceConfig.Connection.Lan.HostUrl, port);
            }

            CheckConnection();
            CheckResult();
        }

        /// <summary>
        /// Отключение устройства
        /// </summary>
        public void Disconnect()
        {
            _driver.Disconnect();
        }

        /// <summary>
        /// Проверка соединения
        /// </summary>
        private void CheckConnection()
        {
            _driver.CheckConnection();
        }

        private void CheckResult()
        {
            int resultCode = _driver.ResultCode();

            if (resultCode == (int)ErrorFP.NoError)
                return;

            string kkmResultDescription = _driver.ResultCodeDescription();

            var errType = resultCode switch
            {
                (int)ErrorFP.CommandError => Enums.ErrorTypes.NoConnection,
                (int)ErrorFP.NoPaper => Enums.ErrorTypes.NoPaper,
                (int)ErrorFP.Session24Hours => Enums.ErrorTypes.SessionMore24Hour,
                _ => Enums.ErrorTypes.Unknown
            };

            Disconnect();
            throw new KkmException(null, errType, resultCode, kkmResultDescription);
        }

        public void GetReport(Enums.ReportTypes report, Cashier cashier)
        {
            if (report == Enums.ReportTypes.ZReport)
            {
                PrintReport('Z');
            }
            else
            {
                PrintReport('X');
            }

            Disconnect();
            CheckResult();
        }

        /// <summary>
        /// Отчёты
        /// </summary>
        /// <param name="reportType">Тип отчета</param>
        private void PrintReport(char reportType)
        {
            // Номер отчета о закрытии смены ; Номер фискального документа ; Фискальный признак
            int nRep, nFD, SignFP;
            // Итоговый оборот в чеках прихода по группе НДС
            double Sum1, Sum2, Sum3, Sum4, Sum5, Sum6, Sum7, Sum8;
            // Итоговый оборот в чеках возвратов прихода по группе НДС
            double Ret1, Ret2, Ret3, Ret4, Ret5, Ret6, Ret7, Ret8;
            // Итоговый оборот в чеках расхода по группе НДС
            double Purch1, Purch2, Purch3, Purch4, Purch5, Purch6, Purch7, Purch8;
            // Итоговый оборот в чеках возвратов расхода по группе НДС
            double RetPurch1, RetPurch2, RetPurch3, RetPurch4, RetPurch5, RetPurch6, RetPurch7, RetPurch8;

            _driver.PrintReport(reportType, out nRep, out nFD, out SignFP, out Sum1, out Sum2, out Sum3, out Sum4, out Sum5, out Sum6, out Sum7, out Sum8, out Ret1, out Ret2, out Ret3, out Ret4, out Ret5, out Ret6, out Ret7, out Ret8, out Purch1, out Purch2, out Purch3, out Purch4, out Purch5, out Purch6, out Purch7, out Purch8, out RetPurch1, out RetPurch2, out RetPurch3, out RetPurch4, out RetPurch5, out RetPurch6, out RetPurch7, out RetPurch8);

            // Сумма в кассе
            //var totalSum = (Sum1 + Sum2 + Sum3 + Sum4 + Sum5 + Sum6 + Sum7 + Sum8) - (Ret1 + Ret2 + Ret3 + Ret4 + Ret5 + Ret6 + Ret7 + Ret8);
        }
        
        public KkmStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public void OpenSession(Cashier cashier)
        {
            OpenShift(cashier);
            CheckResult();
        }

        public void OpenReceipt(ReceiptData? receipt)
        {
            OpenFiscalInv(receipt);
            CheckResult();

            for (int i = 0; i < receipt.Items.Count; i++)
                RegisterItem(receipt.Items[i]);;
            
            for (int i = 0; i < receipt.Payments.Count; i++)
                RegisterPayment(receipt.Payments[i]);
        }

        public void CloseReceipt()
        {
            CloseFiscalInv();
            CheckResult();
        }

        public void CancelReceipt()
        {
            throw new NotImplementedException();
        }

        public void RegisterItem(ReceiptItem item)
        {
            RegisterSale(item);
            CheckResult();
        }

        public void RegisterPayment(ReceiptPayment payment)
        {
            if (payment.Sum == 0)
                return;

            Total(payment.MethodIndex, Convert.ToDouble(payment.Sum));
            CheckResult();
        }

        public void PrintText(string text)
        {
            //throw new NotImplementedException();
        }

        public void CashIn(decimal sum, Cashier cashier)
        {
            ParishOrConsumption(sum);
            CheckResult();
        }

        public void CashOut(decimal sum, Cashier cashier)
        {
            ParishOrConsumption(-sum);
            CheckResult();
        }

        /// <summary>
        /// Открытие смены
        /// </summary>
        /// <param name="cashier">Данные оператора</param>
        /// <param name="clientAddress">Email/телефон клиента</param>
        private void OpenShift(Cashier cashier, string clientAddress = "")
        {
            // Номер оператора
            int opCode = 1;
            // Пароль оператора
            string opPwd = string.Empty;
            // Номер точки продаж
            int tillNmb = 1;
            // E-mail адрес или телефон покупателя для отправки чека в электронном виде
            string buyer = clientAddress;
            // Система налогообложения (СНО)
            string vatSystem = GetTaxSystem();
            // Должность и ФИО кассира
            string CashierName = cashier.Name;
            // ИНН кассира
            string operINN = cashier.TaxId;
            // Номер фискального документа
            int nFD;
            // Фискальный признак
            int SignFP;
            // Номер смены
            int nShift;

            _driver.OpenShift(opCode, opPwd, tillNmb, buyer, vatSystem, CashierName, operINN, out nFD, out SignFP, out nShift);
        }

        /// <summary>
        /// Открытие кассового (клиентского) чека
        /// </summary>
        private void OpenFiscalInv(ReceiptData receipt)
        {
            string clientAddress = string.Empty;
            if (receipt.Contractor != null)
            {
                if (!string.IsNullOrEmpty(receipt.Contractor.Phone))
                {
                    clientAddress = receipt.Contractor.Phone;
                }

                if (!string.IsNullOrEmpty(receipt.Contractor.Email))
                {
                    clientAddress = receipt.Contractor.Email;
                }
            }

            // Номер оператора
            int opCode = 1;
            // Пароль оператора
            string opPwd = string.Empty;
            // Номер точки продаж
            int tillNmb = 1;
            // E-mail адрес или телефон покупателя для отправки чека в электронном виде
            string buyer = clientAddress;
            // Система налогообложения (СНО)
            string vatSystem = GetTaxSystem();
            // Должность и ФИО кассира
            string cashierName = receipt.Cashier.Name;
            // ИНН кассира
            string operINN = receipt.Cashier.TaxId;
            // Сквозной номер документа
            int slipNumber;
            // Текущий номер чека
            int nDoc;
            // Номер смены
            int nShift;

            _driver.OpenFiscalInv(opCode, opPwd, tillNmb, (int)receipt.OperationType, buyer, vatSystem, cashierName, operINN, out slipNumber, out nDoc, out nShift);
        }

        /// <summary>
        /// Регистрация продажи
        /// </summary>
        private void RegisterSale(ReceiptItem item)
        {
            //специфичные для РФ данные
            var ruData = item.CountrySpecificData.Deserialize<ReceiptItemData>();

            string pluName = item.Name;
            byte taxCd = (byte)item.TaxRateIndex;
            double price = Convert.ToDouble(item.Price);
            double quantity = Convert.ToDouble(item.Quantity);
            // Код ед. измерения
            int unit = 0;
            // Номер отдела (секции)
            int department = 1;
            // Тип наценки/скидки
            char discountType = '0';
            // Значение скидки
            double discountValue = 0;
            // Сумма скидки
            double discSum = 0;
            // Сумма
            double sum = Convert.ToDouble(Math.Round(item.Price * item.Quantity, 2, MidpointRounding.AwayFromZero));
            // Признак предмета расчета
            int sign = (int)(ruData == null ? FfdCalculationSubjects.SimpleGood : ruData.FfdData.Subject);
            // Признак способа расчета 
            int signPay = (int)(ruData == null ? FfdCalculationMethods.FullPayment : ruData.FfdData.Method);
            // Тип маркировки
            string markType = string.Empty;
            // GTIN маркированного товара
            string field_1 = string.Empty;
            // Serial маркированного товара
            string field_2 = string.Empty;

            if (ruData != null && ruData.MarkingInfo != null)
            {
                // Маркировка заполнение
            }

            // Код страны происхождения предмета расчета
            string code1230 = string.Empty;
            // Номер таможенной декларации
            string string1231 = string.Empty;
            // Доп. реквизит для предмета расчета
            string tag1191 = string.Empty;
            // Сумма акциза
            string excise = string.Empty;
            // Сумма НДС
            double vat = GetVatsum(sum, item.TaxRateIndex);
           
            // Сквозной номер документа
            int slipNumber;

            _driver.RegisterSale(pluName, taxCd, price, quantity, unit, department, discountType, discountValue, discSum, sum, sign, signPay, markType, field_1, field_2, code1230, string1231, tag1191, excise, vat, out slipNumber);
        }

        /// <summary>
        /// Оплата (итог)
        /// </summary>
        private void Total(int paidMode, double amount)
        {
            // Используется при работе с иностранной валютой
            byte change = 0;
            // Указывает на ошибку или ее отсутствие
            byte myErrorStatus;
            // Оставшаяся для оплаты сумма
            double newAmount;
            // Сквозной номер документа
            int slipNumber;

            _driver.Total(paidMode, amount, change, out myErrorStatus, out newAmount, out slipNumber);
        }

        /// <summary>
        /// Закрытие фискального чека 
        /// </summary>
        private void CloseFiscalInv()
        {
            // Сквозной номер документа
            int slipNumber;
            // Номер фискального документа
            int nFD;
            // Фискальный признак
            string signFP;
            // Текущий номер чека
            int nDoc;
            // Номер смены
            int nShift;

            _driver.CloseFiscalInv(out slipNumber, out nFD, out signFP, out nDoc, out nShift);
        }

        /// <summary>
        /// Служебный приход или расход
        /// </summary>
        private void ParishOrConsumption(decimal sum)
        {
            // Тип операции
            char type = sum > 0 ? '0' : '1';
            // Сумма для внесения\выплаты
            double amount = Convert.ToDouble(Math.Abs(sum));
            // Сумма наличных денег в ящике
            double cashSum;
            // Итоговая сумма операций внесения
            double cashIn;
            // Итоговая сумма операций выплаты
            double cashOut;

            _driver.ParishOrConsumption(type, amount, out cashSum, out cashIn, out cashOut);
        }


        /// <summary>
        /// Аннуляция (сторно) фискального чека
        /// </summary>
        private void CancelFiscalInv()
        {
            // Сквозной номер документа
            int slipNumber;
            _driver.CancelFiscalInv(out slipNumber);
        }

        /// <summary>
        /// Обрезка ленты
        /// </summary>
        /// <remarks>При вызове данного метода на ККТ производится отрезка ленты (только для моделей ПОРТ-600Ф и ПОРТ-1000Ф).</remarks>
        private void CutPaper()
        {
            _driver.CutPaper();
        }

        public void OpenCashDrawer(int impulse = 500)
        {
            _driver.OpenCashDrawer(impulse);
        }

        /// <summary>
        /// Получение суммы НДС
        /// </summary>
        /// <param name="vatCode">Код группы НДС</param>
        /// <param name="sum">Сумма</param>
        private double GetVatsum(double sum, int? vatCode = 4)
        {
            switch (vatCode)
            {
                // Ставка НДС 20%
                case 1:
                    return sum * 20 / (100 + 20);
                // Ставка НДС 10%
                case 2:
                    return sum * 20 / (100 + 20);
                // Ставка НДС 0%
                case 3:
                    break;
                // НДС не облагается
                case 4:
                    break;
                // Ставка НДС расчётная 20/120
                case 5:
                    return sum * 20 / 120;
                // Ставка НДС расчётная 10/110
                case 6:
                    return sum * 20 / 120;

                default:
                    break;
            }

            return 0;
        }

        /// <summary>
        /// Получить код СНО
        /// </summary>
        /// <returns></returns>
        private string GetTaxSystem(int taxSystem = 6)
        {
            return taxSystem switch
            {
                // ОСН
                1 => "1",
                // УСН доход
                2 => "2",
                // УСН доход-расход
                3 => "4",
                // ЕНВД
                4 => "8",
                // ЕСН
                5 => "16",
                // Патент
                6 => "32",
                // По умолчанию всегда патент
                _ => "32",
            };
        }

        /// <summary>
        /// Список кодов ошибок возвращаемых устройством
        /// </summary>
        private enum ErrorFP
        {
            NoError = 0,
            /// <summary>
            /// Ошибочный ввод/вывод!
            /// </summary>
            CommandError = 100001,
            /// <summary>
            /// Операция невозможна
            /// </summary>
            NotPossible = 111003,
            /// <summary>
            /// Нет наличных! 
            /// </summary>
            NoCashInFP = 111017,
            /// <summary>
            /// Сумма чека равна нулю
            /// </summary>
            DocSumZero = 111050,
            /// <summary>
            /// // Нет бумаги
            /// </summary>
            NoPaper = 112006,
            /// <summary>
            /// Смена превысила 24 часа
            /// </summary>
            Session24Hours = 111024,
            /// <summary>
            /// Смена открыта
            /// </summary>
            SessionOpen = 110108,
            /// <summary>
            /// Смена закрыта
            /// </summary>
            SessionClose = 110109,
            /// <summary>
            /// Открыт чек прихода - операция не возможна.
            /// </summary>
            FiscalReceiptOpen = 111015,
            /// <summary>
            /// Ошибка в режиме регистрации: открыт нефискальный чек 
            /// </summary>
            ServiceReceiptOpen = 111046,
            /// <summary>
            /// Касса не зарегистрирована
            /// </summary>
            NoRegister = 110209,
            /// <summary>
            /// Неопознанная ошибка, возникающая при снятии Z отчёта
            /// </summary>
            Error110969 = 110969,
            /// <summary>
            /// Неверное значение параметра 1.
            /// </summary>
            FP_BAD_PARAM_1 = 112201,
            /// <summary>
            /// Код товара не существует!
            /// </summary>
            GoodNoFind = 111004
        }

        #region Dispose
        private bool disposed = false;

        // реализация интерфейса IDisposable.
        public void Dispose()
        {
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Освобождаем управляемые ресурсы
                }
                // освобождаем неуправляемые объекты
                disposed = true;
            }
        }

        // Деструктор
        ~PortDriverRu()
        {
            Dispose(false);
        }
        #endregion
    }
}
