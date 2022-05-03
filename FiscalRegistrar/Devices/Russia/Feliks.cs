using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;
using devicesConnector.Helpers;
using System.Text;
using System.Text.Json;
using static devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices.Russia
{
    public class Feliks : IFiscalRegistrarDevice
    {
        private readonly Device _deviceConfig;
        /// <summary>
        /// Ответ устройства
        /// </summary>
        private byte[] response = null;
        // Тип операции
        private ReceiptOperationTypes operType;

        #region Константы
        /// <summary>
        /// Пароль по умолчанию
        /// </summary>
        private const byte DefaultPassword = 0x30;
        /// <summary>
        /// [ENQ] Запрос
        /// </summary>
        private const byte Request = 0x05;
        /// <summary>
        /// [ACK] Подтверждение
        /// </summary>
        private const byte Confirm = 0x06;
        /// <summary>
        /// Начало текста
        /// </summary>
        private const byte STX = 0x02;
        /// <summary>
        /// Конец текста
        /// </summary>
        private const byte ETX = 0x03;
        /// <summary>
        /// Конец передачи
        /// </summary>
        private const byte EOT = 0x04;
        /// <summary>
        /// Экранирование управляющих символов
        /// </summary>
        private const byte DLE = 0x10;
        private const byte EMPTY = 0x00;
        #endregion

        public Feliks(Device device)
        {
            _deviceConfig = device;
        }

        public void Connect()
        {
            // Внимание для снятия X/Z отчёта
            // таймаут может потребовать больше, например 20000мс

            if(_deviceConfig.Connection.ComPort == null)
                throw new NullReferenceException();

            _deviceConfig.Connection.ComPort.SetSerialPort();

            if(!_deviceConfig.Connection.ComPort.Open())
                throw new NullReferenceException();
        }

        private bool CheckConnection()
        {
            if (_deviceConfig.Connection.ComPort == null)
                throw new NullReferenceException();

            response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { Request });
            if (response == null)
            {
                LogHelper.Write($"{_deviceConfig.Name}. Нет связи с устройством. Нет ответа в отведенный промежуток времени.");
                Disconnect();
                return false;
            }

            if (response[0] != Confirm)
            {
                LogHelper.Write($"{_deviceConfig.Name}. Нет связи с устройством. Нет подтверждения в отведенный промежуток времени.");
                Disconnect();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Отключение устройства
        /// </summary>
        public void Disconnect()
        {
            if(_deviceConfig.Connection.ComPort == null)
                throw new NullReferenceException();

            if (!_deviceConfig.Connection.ComPort.Close())
                throw new NullReferenceException();
        }

        public KkmStatus GetStatus()
        {
            throw new NotImplementedException();
        }

        public void OpenSession(Cashier cashier)
        {
            CommandOpenShift(cashier);
        }

        public void GetReport(Enums.ReportTypes report, Cashier cashier)
        {
            if (report == Enums.ReportTypes.ZReport)
            {
                CommandReportZ(cashier);
            }
            else
            {
                CommandReportX(cashier);
            }

            Disconnect();
            CheckResult();
        }

        public void CashIn(decimal sum, Cashier cashier)
        {
            int tempSum = Convert.ToInt32(sum * 100);
            if (!CommandMoneyInOut(tempSum, cashier))
            {
                LogHelper.Write($"{_deviceConfig.Name}. Не удалось выполнить операцию внесения.");
                return;
            }

            LogHelper.Write($"{_deviceConfig.Name}. Операция внесения на сумму {sum:F2} выполнена успешно.");
        }

        public void CashOut(decimal sum, Cashier cashier)
        {
            int tempSum = Convert.ToInt32(sum * 100);
            if (!CommandMoneyInOut(-tempSum, cashier))
            {
                LogHelper.Write($"{_deviceConfig.Name}. Не удалось выполнить операцию изиятия.");
                return;
            }

            LogHelper.Write($"{_deviceConfig.Name}. Операция изъятия на сумму {sum:F2} выполнена успешно.");
        }

        public void OpenReceipt(ReceiptData? receipt)
        {
            if (!CommandOpenCheck(receipt))
                return;

            CommandClientAddress(receipt);
            operType = receipt.OperationType;
        }

        public void RegisterItem(ReceiptItem item)
        {
            CommandRegistrationPosition(item);
        }

        public void RegisterPayment(ReceiptPayment receiptPayment)
        {
            int paySum = Convert.ToInt32(receiptPayment.Sum * 100);
            CommandPay(receiptPayment.MethodIndex, paySum);
        }

        public void CloseReceipt()
        {
            CommandCloseCheck();
        }

        public void CancelReceipt()
        {
            CommandCancelCheck();
        }

        public void CutPaper()
        {
            LogHelper.Write($"{_deviceConfig.Name}. Отрезка не реализована.");
        }

        public void PrintText(string textPrint)
        {
            CommandPrintText(textPrint);
        }

        public void OpenCashBox()
        {
            LogHelper.Write($"{_deviceConfig.Name}. Открытие денежного ящика не реализована.");
        }

        /// <summary>
        /// Команда снятия X отчёта
        /// </summary>
        private bool CommandReportX(Cashier cashier)
        {
            if (!CheckConnection())
                return false;

            if (!CommandChangeMode(2))
                return false;

            if (!IsToDeviceWrite())
                return false;

            CommandOperator(cashier);
            CommandOperator(cashier, true);

            // Оборот по текущей смене без закрытия
            const byte g = 0x67;
            // Тип Отчёта: 1 – Оборот по текущей смена без закрытия
            const int typeReport = 1;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, g, typeReport });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Команда снятия Z отчёта
        /// </summary>
        private bool CommandReportZ(Cashier cashier)
        {
            if (!CheckConnection())
                return false;

            if (!CommandChangeMode(3))
                return false;

            if (!IsToDeviceWrite())
                return false;

            CommandOperator(cashier);
            CommandOperator(cashier, true);

            // Оборот по текущей смене с закрытием
            const byte Z = 0x5A;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, Z });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Комманда на смену режима работы
        /// </summary>
        /// <param name="mode">Номер режима</param>
        /// <returns>В случае успешной смены вернёт true, иначе вернёт false</returns>
        private bool CommandChangeMode(int mode)
        {
            // Вход в режим
            const byte V = 0x56;
            // Устанавливаемые режимы (двоично-десятичное число):
            // 1 - режим регистрации;
            // 2 - оборота по текущей смена без закрытия;
            // 3 - оборота по текущей смена с закрытием;
            // 4 - режим программирования;
            // 5 - режим налогового инспектора;

            if (GetCurrentCode(mode) == mode)
                return true;

            if (!IsToDeviceWrite())
                return false;

            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, V, (byte)mode, EMPTY, EMPTY, EMPTY, DefaultPassword });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return true;
        }

        /// <summary>
        /// Получить текущее состояние ККТ
        /// </summary>
        /// <param name="mode"></param>
        private int GetCurrentCode(int mode)
        {
            // Запрос кода состояния ККТ
            const byte E = 0x45;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, E });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return 0;

            GetPreparedResponse(response);

            CheckResult();
            if (response[2] == mode)
                return mode;

            if (response[2] == 0)
                return response[2];

            CommandExitMode();
            return 0;
        }

        /// <summary>
        /// Команда открытия смены
        /// </summary>
        private bool CommandOpenShift(Cashier cashier)
        {
            if (!CheckConnection())
                return false;

            if (!CommandChangeMode(1))
                return false;

            if (!IsToDeviceWrite())
                return false;

            CommandOperator(cashier);
            CommandOperator(cashier, true);

            // Открыть смену
            const byte SHIFT = 0x9A;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, SHIFT });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Комманда печати наименования товара (услуги)
        /// </summary>
        private bool CommandPrintText(string textPrint, byte mode = 0x4C)
        {
            // Печатаемые символы (X до 96 символов) - кодовой странице 866 MSDOS. 
            textPrint = textPrint.Length > 96 ? textPrint.Substring(0, 96) : textPrint;

            // Открытие чека
            byte TEXT = mode;
            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, TEXT });
            tempCommand.AddRange(Encoding.GetEncoding(866).GetBytes(textPrint));
            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        private bool CommandOpenCheck(ReceiptData receipt)
        {
            CommandOperator(receipt.Cashier);
            CommandOperator(receipt.Cashier, true);

            byte typeCheck = (byte)(receipt.OperationType == Enums.ReceiptOperationTypes.Sale ? 1 : 2);
            // Открытие чека
            const byte CHECK = 0x92;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, CHECK, EMPTY, (byte)typeCheck });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        private bool CommandClientAddress(ReceiptData receipt)
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

            if (string.IsNullOrWhiteSpace(clientAddress))
                return true;

            byte mode = EMPTY;
            if (clientAddress.Contains("@"))
                mode = 0x02;
            else
            {
                // Первым символом в номере обязательно должен быть символ «+»
                clientAddress = clientAddress.Contains("+") ? clientAddress : $"+{clientAddress}";
            }

            // Звонок на номер телефона
            const byte SMS = 0x70;
            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, SMS });
            tempCommand.Add(mode);
            tempCommand.AddRange(Encoding.GetEncoding(866).GetBytes(clientAddress));

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        private bool CommandOperator(Cashier cashier, bool isTaxId = false)
        {
            byte type;
            string cashierData;
            if (isTaxId)
            {
                if (string.IsNullOrWhiteSpace(cashier.TaxId))
                    return true;

                type = 0x1C;
                cashierData = cashier.TaxId;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(cashier.Name))
                    return true;

                type = 0x19;
                cashierData = cashier.Name;
            }

            // Звонок на номер телефона
            const byte USER = 0x7E;
            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, USER, type });
            tempCommand.AddRange(Encoding.GetEncoding(866).GetBytes(cashierData));

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Команда внесения/выплаты денег
        /// </summary>
        private bool CommandMoneyInOut(int sum, Cashier cashier)
        {
            if (!CheckConnection())
                return false;

            if (!CommandChangeMode(1))
                return false;

            if (!IsToDeviceWrite())
                return false;

            CommandOperator(cashier);
            CommandOperator(cashier, true);

            // Внесение денег
            const byte MONEYIN = 0x49;
            // Выплата денег
            const byte MONEYOUT = 0x4F;

            byte currentMode = MONEYIN;
            if (sum < 0)
            {
                currentMode = MONEYOUT;
                sum = Math.Abs(sum);
            }

            byte[] currentSum = GetDigit(sum);

            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, currentMode, EMPTY });
            tempCommand.AddRange(currentSum);

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Команда выхода из текущего режима
        /// </summary>
        private void CommandExitMode()
        {
            IsToDeviceWrite();

            // Выход из текущего режима
            const byte R = 0x52;

            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, R });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            IsFromDeviceRead();
        }

        /// <summary>
        /// Команда аннулирования чека
        /// </summary>
        private bool CommandCancelCheck()
        {
            // Аннулирование чека
            const byte CANCEL = 0x59;
            var command = GetPreparedCommand(new byte[] { EMPTY, EMPTY, CANCEL });
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();            
            LogHelper.Write($"{_deviceConfig.Name}. Выполнено аннулирование ранее открытого чека.");
            return true;
        }

        /// <summary>
        /// Команда оплаты
        /// </summary>
        private bool CommandPay(int type, int sum)
        {
            // Тип оплаты
            byte payType = GetPayType(type);
            byte[] paySum = GetDigit(sum);

            // Расчёт по чеку
            const byte PAY = 0x99;
            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, PAY, EMPTY });
            tempCommand.Add(payType);
            tempCommand.AddRange(paySum);

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Команда закрытия чека
        /// </summary>
        private bool CommandCloseCheck()
        {
            // Закрытие чека
            const byte CLOSE = 0x4A;
            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, CLOSE, EMPTY, EMPTY });

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        /// <summary>
        /// Команда регистрации позиции прихода/расхода
        /// </summary>
        private bool CommandRegistrationPosition(ReceiptItem item)
        {
            //специфичные для РФ данные
            var ruData = item.CountrySpecificData.Deserialize<ReceiptItemData>();

            byte[] price = GetDigit(Convert.ToInt32(item.Price * 100));
            byte[] quantity = GetDigit(Convert.ToInt32(item.Quantity * 1000));
            byte department = EMPTY;
            byte discount = 0x01;
            byte[] vatValue = { 0x00, 0x00, 0x00 };
            byte[] discountSum = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte vatGroupCode = GetVatGroup(item.TaxRateIndex);
            byte[] vatSum = { 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] Sum = GetDigit(Convert.ToInt32(Math.Round(item.Price * item.Quantity, 2, MidpointRounding.AwayFromZero) * 100));
            // Признак предмета расчета
            byte sing = (byte)(ruData == null ? FfdCalculationSubjects.SimpleGood : ruData.FfdData.Subject);
            // Признак способа расчёта
            // На текущий момент полный расчёт используется
            byte signPay = 0x04; /*(int)(ruData == null ? FfdCalculationMethods.FullPayment : ruData.FfdData.Method);*/

            // Регистрация прихода/расхода
            byte REGISTRATION = 0x48;
            if (operType == ReceiptOperationTypes.ReturnSale)
                REGISTRATION = 0x57;

            var tempCommand = new List<byte>();
            tempCommand.AddRange(new byte[] { EMPTY, EMPTY, REGISTRATION, EMPTY });
            tempCommand.AddRange(price);
            tempCommand.AddRange(quantity);
            tempCommand.Add(department);
            tempCommand.Add(discount);
            tempCommand.AddRange(vatValue);
            tempCommand.AddRange(discountSum);
            tempCommand.Add(vatGroupCode);
            tempCommand.AddRange(vatSum);
            tempCommand.AddRange(Sum);
            tempCommand.Add(sing);
            tempCommand.Add(signPay);

            var command = GetPreparedCommand(tempCommand.ToArray());
            // Отправляем основную команду и ожидаем подтверждения
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(command);

            if (!IsFromDeviceRead())
                return false;

            CheckResult();
            return IsToDeviceWrite();
        }

        #region Специальные

        /// <summary>
        /// От устройства
        /// </summary>
        private bool IsFromDeviceRead()
        {
            if (response == null)
                return false;

            // Проверяем подтвердило ли устройство получение
            if (response[0] != Confirm)
                return false;

            // Завершаем передачу
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { EOT });
            // Проверяем есть ли запрос от устройства
            if (response[0] != Request)
                return false;

            // Подтверждаем получение запроса и получаем новые данные
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { Confirm }, 6);
            return true;
        }

        /// <summary>
        /// К устройству
        /// </summary>
        private bool IsToDeviceWrite()
        {
            // Отправляем подтверждение о получении
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { Confirm });
            // Проверяем завершена ли передача с устройством
            if (response[0] != EOT)
                return false;
            // Отправляем запрос
            response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { Request });
            // Проверяем подтвердило ли устройство получение
            return response[0] == Confirm;
        }

        /// <summary>
        /// Получить подготовленную комманду
        /// </summary>
        /// <param name="dataWrite"></param>
        /// <returns></returns>
        private byte[] GetPreparedCommand(byte[] dataWrite)
        {
            var command = new List<byte>();
            for (int i = 0; i < dataWrite.Length; i++)
            {
                // Экранируем специальные команды
                if (dataWrite[i] == ETX)
                    command.AddRange(new byte[] { DLE, dataWrite[i] });
                else if (dataWrite[i] == DLE)
                    command.AddRange(new byte[] { DLE, DLE });
                else
                    command.Add(dataWrite[i]);
            }
            command.Add(ETX);

            var crc = CalcCRC(command.ToArray());
            command.Insert(0, STX);
            command.Insert(command.Count, crc);

            return command.ToArray();
        }

        /// <summary>
        /// Получить подготовленный ответ
        /// </summary>
        /// <param name="dataRead"></param>
        private void GetPreparedResponse(byte[] dataRead)
        {
            var tempResponse = new List<byte>();
            for (int i = 0; i < dataRead.Length; i++)
            {
                if (dataRead[i] != DLE)
                {
                    tempResponse.Add(dataRead[i]);
                    continue;
                }
                int index = i + 1 > dataRead.Length ? i : i + 1;
                if (dataRead[index] == ETX)
                    continue;

                tempResponse.Add(dataRead[i]);
            }

            response = tempResponse.ToArray();
        }

        /// <summary>
        /// Расчёт контрольного числа
        /// </summary>
        private byte CalcCRC(byte[] command)
        {
            int result = 0;
            for (int i = 0; i < command.Length; i++)
            {
                result ^= command[i];
            }

            return Convert.ToByte(result);
        }

        /// <summary>
        /// Разбить строку на определенные разряды
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="size"></param>
        /// <param name="fixedSize"></param>
        /// <remarks>https://ru.stackoverflow.com/a/478202/190858</remarks>
        IEnumerable<string> Split(TextReader sr, int size, bool fixedSize = true)
        {
            while (sr.Peek() >= 0)
            {
                var buffer = new char[size];
                var c = sr.ReadBlock(buffer, 0, size);
                yield return fixedSize ? new String(buffer) : new String(buffer, 0, c);
            }
        }

        IEnumerable<string> Split(string s, int size, bool fixedSize = true)
        {
            var sr = new StringReader(s);
            return Split(sr, size, fixedSize);
        }

        /// <summary>
        /// Получить число
        /// </summary>
        private byte[] GetDigit(int data)
        {
            string TempValue = string.Format("{0:D10}", data);
            var result = Split(TempValue, 2).Select(s => Convert.ToByte(s, 16)).ToArray();
            return result;
        }

        #endregion

        private bool CheckResult()
        {
            var errType = Enums.ErrorTypes.Unknown;
            int resultCode = -1;

            string kkmResultDescription = string.Empty;

            if (response.Length < 3)
                throw new KkmException(null, errType, resultCode, kkmResultDescription);

            if (response[0] != STX)
                throw new KkmException(null, errType, resultCode, kkmResultDescription);

            if (response[1] == 0x00)
                return true;

            if (response[1] != 0x55)
                throw new KkmException(null, errType, resultCode, kkmResultDescription);

            switch (response[2])
            {
                case 0x00:
                    return true;
                case 0x01:
                case 0x02:
                case 0x03:
                    return true;
                case 0xE8:
                    resultCode = (int)Enums.ErrorTypes.SessionMore24Hour;
                    kkmResultDescription = $"{_deviceConfig.Name}. Смена превысила 24 часа.";
                    // Отправляем подтверждение о получении
                    response = _deviceConfig.Connection.ComPort.ExecuteCommand(new byte[] { Confirm });
                    break;
                case 0x6A:
                    resultCode = (int)Enums.ErrorTypes.NonCorrectData;
                    kkmResultDescription = $"{_deviceConfig.Name}. Неверный тип чека.";
                    break;
                case 0x67:
                    resultCode = (int)Enums.ErrorTypes.NoPaper;
                    kkmResultDescription = $"{_deviceConfig.Name}. Отсутствует бумага.";
                    break;
                case 0x76:
                    resultCode = (int)Enums.ErrorTypes.SessionClose;
                    kkmResultDescription = $"{_deviceConfig.Name}. Смена закрыта операция не возможна.";
                    break;
                case 0x8C:
                    resultCode = (int)Enums.ErrorTypes.PasswordIncorrect;
                    kkmResultDescription = $"{_deviceConfig.Name}. Устройство вернуло не опознанную ошибку. Возможно не верный пароль кассира.";
                    break;
                case 0x9A:
                    resultCode = (int)Enums.ErrorTypes.ReceiptClose;
                    kkmResultDescription = $"{_deviceConfig.Name}. Устройство вернуло не опознанную ошибку. Возможно чек закрыт и операция невозможна.";
                    break;
                case 0x9C:
                    resultCode = (int)Enums.ErrorTypes.SessionOpen;
                    kkmResultDescription = $"{_deviceConfig.Name}. Смена уже открыта.";
                    return true;
                case 0x9B:
                    resultCode = (int)Enums.ErrorTypes.FiscalReceiptOpen;
                    kkmResultDescription = $"{_deviceConfig.Name}. Есть открытый чек, будет предпринята попытка аннулирования.";
                    LogHelper.Write(kkmResultDescription);

                    if (!IsToDeviceWrite())
                    {
                        kkmResultDescription = $"{_deviceConfig.Name}. Есть открытый чек. Не удалось аннулировать ранее открытый чек.";
                        break;
                    }

                    if (!CommandCancelCheck())
                    {
                        kkmResultDescription = $"{_deviceConfig.Name}. Есть открытый чек. Не удалось аннулировать ранее открытый чек.";
                        break;
                    }

                    return true;
                case 0x66:
                    resultCode = (int)Enums.ErrorTypes.CommandIncorrect;
                    kkmResultDescription = $"{_deviceConfig.Name}. Команда не реализуется в данном режиме ККТ.";
                    break;

                default:
                    resultCode = (int)Enums.ErrorTypes.Unknown;
                    kkmResultDescription = $"{_deviceConfig.Name}. Устройство вернуло не опознанную ошибку.";
                    break;
            }

            LogHelper.Write(kkmResultDescription);
            throw new KkmException(null, errType, resultCode, kkmResultDescription);
            //Disconnect();
        }

        /// <summary>
        /// Получить тип оплаты для устройства
        /// </summary>
        /// <param name="payType">Тип оплаты</param>
        private byte GetPayType(int payType)
        {
            switch (payType)
            {
                case 1:
                    return 0x01;
                case 3:
                    return 0x02;

                default:
                    return 0x01;
            }
        }

        /// <summary>
        /// Получить код группы НДС для ФР
        /// </summary>
        private byte GetVatGroup(int? vatGroupCode)
        {
            switch (vatGroupCode)
            {
                // Ставка НДС 20%
                case 1:
                    return 0x01;
                // Ставка НДС 10%
                case 2:
                    return 0x02;
                // Ставка НДС 0%
                case 3:
                    return 0x05;
                // НДС не облагается
                case 4:
                    return 0x06;
                // Ставка НДС расчётная 20/120
                case 5:
                    return 0x03;
                // Ставка НДС расчётная 10/110
                case 6:
                    return 0x04;

                // НДС не облагается
                default:
                    return 0x06;
            }
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
        ~Feliks()
        {
            Dispose(false);
        }
        #endregion
    }
}
