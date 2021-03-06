﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.Bluetooth;
using Android.Util;

using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace EcoBand {
    public class Band {
        public Band(IDevice device) {
            Device = device;

            _eventHandlers = new HashSet<EventHandler<CharacteristicUpdatedEventArgs>>();
        }


        /**************************************************************************

            Static properties

         **************************************************************************/

        public static readonly List<string> NAME_FILTER = new List<string>() {
            "MI",
            "MI1S",
            "MI1A",
            "MI1"
        };

        public static readonly List<string> MAC_ADDRESS_FILTER = new List<string>() {
            "88:0F:10",
            "C8:0F:10"
        };


        /**************************************************************************

            Public properties

         **************************************************************************/

        public readonly IDevice Device;
        public event EventHandler<MeasureEventArgs> Steps;
        public event EventHandler<MeasureEventArgs> HeartRate;


        /**************************************************************************

            Private properties

         **************************************************************************/

        // Services
        private readonly Guid UUID_SV_MAIN = Guid.Parse("0000fee0-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_SV_GENERIC_SERVICE = Guid.Parse("00001800-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_SV_GENERIC_ATTRIBUTE = Guid.Parse("00001801-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_SV_VIBRATION = Guid.Parse("00001802-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_SV_HEART_RATE = Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_SV_WEIGHT = Guid.Parse("00001530-0000-3512-2118-0009af100700");


        // Characteristics
        private readonly Guid UUID_CH_DEVICE_INFO = Guid.Parse("0000ff01-0000-1000-8000-00805f9b34fb"); // read
        private readonly Guid UUID_CH_DEVICE_NAME = Guid.Parse("0000ff02-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_NOTIFICATION = Guid.Parse("0000ff03-0000-1000-8000-00805f9b34fb"); // read notify
        private readonly Guid UUID_CH_USER_INFO = Guid.Parse("0000ff04-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_CONTROL_POINT = Guid.Parse("0000ff05-0000-1000-8000-00805f9b34fb"); // write
        private readonly Guid UUID_CH_REALTIME_STEPS = Guid.Parse("0000ff06-0000-1000-8000-00805f9b34fb"); // read notify
        private readonly Guid UUID_CH_ACTIVITY_DATA = Guid.Parse("0000ff07-0000-1000-8000-00805f9b34fb"); // read indicate
        private readonly Guid UUID_CH_FIRMWARE_DATA = Guid.Parse("0000ff08-0000-1000-8000-00805f9b34fb"); // write without response
        private readonly Guid UUID_CH_LE_PARAMS = Guid.Parse("0000ff09-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_DATE_TIME = Guid.Parse("0000ff0a-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_STATISTICS = Guid.Parse("0000ff0b-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_BATTERY = Guid.Parse("0000ff0c-0000-1000-8000-00805f9b34fb"); // read notify
        private readonly Guid UUID_CH_TEST = Guid.Parse("0000ff0d-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_SENSOR_DATA = Guid.Parse("0000ff0e-0000-1000-8000-00805f9b34fb"); // read notify
        private readonly Guid UUID_CH_PAIR = Guid.Parse("0000ff0f-0000-1000-8000-00805f9b34fb"); // read write
        private readonly Guid UUID_CH_VIBRATION = Guid.Parse("00002a06-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_CH_HEART_RATE = Guid.Parse("00002a37-0000-1000-8000-00805f9b34fb");
        private readonly Guid UUID_CH_HEART_RATE_CONTROL_POINT = Guid.Parse("00002a39-0000-1000-8000-00805f9b34fb");


        // Descriptors
        private readonly Guid UUID_DC_NOTIFY_CHARACTERISTIC_DETECTION = Guid.Parse("00002902-0000-1000-8000-00805f9b34fb");


        // Commands to send to UUID_CH_HEART_RATE_CONTROL_POINT
        private readonly byte[] HR_CP_START_HEART_RATE_SLEEP = { 0x15, 0x0, 01 };
        private readonly byte[] HR_CP_STOP_HEART_RATE_SLEEP = { 0x15, 0x0, 0 };
        private readonly byte[] HR_CP_START_HEART_RATE_CONTINUOUS = { 0x15, 0x1, 1 };
        private readonly byte[] HR_CP_STOP_HEART_RATE_CONTINUOUS = { 0x15, 0x1, 0 };
        private readonly byte[] HR_CP_START_HEART_RATE_MANUAL = { 0x15, 0x2, 1 };
        private readonly byte[] HR_CP_STOP_HEART_RATE_MANUAL = { 0x15, 0x2, 0 };


        // Commands to send to UUID_CH_CONTROL_POINT
        private readonly byte[] CP_STOP_VIBRATION = { 0x0 };
        private readonly byte[] CP_START_VIBRATION_WITH_LED = { 0x1 };
        private readonly byte[] CP_PAIR = { 0x2 };
        private readonly byte[] CP_START_REALTIME_STEPS = { 0x3, 0x1 };
        private readonly byte[] CP_STOP_REALTIME_STEPS = { 0x3, 0x1 };
        private readonly byte[] CP_SET_ALARM = { 0x4 };
        private readonly byte[] CP_SET_GOAL = { 0x5 };
        private readonly byte[] CP_GET_ACTIVITY_DATA = { 0x6 };
        private readonly byte[] CP_SEND_FIRMWARE_INFO = { 0x7 };
        private readonly byte[] CP_SEND_NOTIFICATION = { 0x8 };
        private readonly byte[] CP_FACTORY_RESET = { 0x9 };
        private readonly byte[] CP_SET_REALTIME_STEPS = { 0x10 };
        private readonly byte[] CP_STOP_SYNC = { 0x11 };
        private readonly byte[] CP_NOTIFY_SENSOR_DATA = { 0x12 };
        //  readonly byte[] CP_STOP_VIBRATION = { 0x13 };
        private readonly byte[] CP_CONFIRM_SYNC_COMPLETED = { 0xA };
        private readonly byte[] CP_SYNC = { 0xB };
        private readonly byte[] CP_REBOOT = { 0xC };
        private readonly byte[] CP_SET_THEME = { 0xE };
        private readonly byte[] CP_SET_WEAR_LOCATION = { 0xF };


        // Commands to send to UUID_CH_TEST
        private readonly byte[] TEST_REMOTE_DISCONNECT = { 0x1 };
        private readonly byte[] TEST_SELFTEST = { 0x2 };
        private readonly byte[] TEST_NOTIFICATION = { 0x3 };
        private readonly byte[] TEST_WRITE_MD5 = { 0x4 };
        private readonly byte[] TEST_DISCONNECTED_REMINDER = { 0x5 };


        // Responses from UUID_CH_NOTIFICATION
        private readonly byte NOTIFICATION_NORMAL = 0x0;
        private readonly byte NOTIFICATION_FIRMWARE_UPDATE_FAILED = 0x1;
        private readonly byte NOTIFICATION_FIRMWARE_UPDATE_SUCCESS = 0x2;
        private readonly byte NOTIFICATION_CONNECTION_PARAM_UPDATE_FAILED = 0x3;
        private readonly byte NOTIFICATION_CONNECTION_PARAM_UPDATE_SUCCESS = 0x4;
        private readonly byte NOTIFICATION_AUTHENTICATION_SUCCESS = 0x5;
        private readonly byte NOTIFICATION_AUTHENTICATION_FAILED = 0x6;
        private readonly byte NOTIFICATION_FITNESS_GOAL_ACHIEVED = 0x7;
        private readonly byte NOTIFICATION_SET_LATENCY_SUCCESS = 0x8;
        private readonly byte NOTIFICATION_RESET_AUTHENTICATION_FAILED = 0x9;
        private readonly byte NOTIFICATION_RESET_AUTHENTICATION_SUCCESS = 0xa;
        private readonly byte NOTIFICATION_FW_CHECK_FAILED = 0xb;
        private readonly byte NOTIFICATION_FW_CHECK_SUCCESS = 0xc;
        private readonly byte NOTIFICATION_STATUS_MOTOR_NOTIFY = 0xd;
        private readonly byte NOTIFICATION_STATUS_MOTOR_CALL = 0xe;
        private readonly byte NOTIFICATION_STATUS_MOTOR_DISCONNECTED = 0xf;
        private readonly byte NOTIFICATION_STATUS_MOTOR_SMART_ALARM = 0x10;
        private readonly byte NOTIFICATION_STATUS_MOTOR_ALARM = 0x11;
        private readonly byte NOTIFICATION_STATUS_MOTOR_GOAL = 0x12;
        private readonly byte NOTIFICATION_STATUS_MOTOR_AUTH = 0x13;
        private readonly byte NOTIFICATION_STATUS_MOTOR_SHUTDOWN = 0x14;
        private readonly byte NOTIFICATION_STATUS_MOTOR_AUTH_SUCCESS = 0x15;
        private readonly byte NOTIFICATION_STATUS_MOTOR_TEST = 0x16;
        private readonly byte NOTIFICATION_DATA_SYNC_CANCELED = 0x18;
        private readonly sbyte NOTIFICATION_UNKNOWN = Convert.ToSByte(-0x1);
        private readonly byte NOTIFICATION_PAIR_CANCELED = 0xef;
        private readonly byte NOTIFICATION_DEVICE_MALFUNCTION = 0xff;


        // Responses from UUID_CH_BATTERY
        private readonly byte BATTERY_NORMAL = 0;
        private readonly byte BATTERY_LOW = 1;
        private readonly byte BATTERY_CHARGING = 2;
        private readonly byte BATTERY_CHARGING_FULL = 3;
        private readonly byte BATTERY_CHARGE_OFF = 4;


        private static IService _mainService;
        private static HashSet<EventHandler<CharacteristicUpdatedEventArgs>> _eventHandlers;


        /**************************************************************************

            Public methods

         **************************************************************************/

        public async Task<int> GetCurrentSteps() {
            byte[] steps;

            try {
                if (_mainService == null) _mainService = await Device.GetServiceAsync(UUID_SV_MAIN);

                steps = await ReadFromCharacteristic(UUID_CH_REALTIME_STEPS, _mainService);

                return DecodeSteps(steps);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error getting steps: {ex.Message}");

                return -1;
            }
        }

        public async Task<bool> StartMeasuringSteps() {
            bool suscribed;
            bool startedMeasuring;

            try {
                if (_mainService == null) _mainService = await Device.GetServiceAsync(UUID_SV_MAIN);

                suscribed = await SubscribeTo(UUID_CH_REALTIME_STEPS, _mainService, OnSteps);
                startedMeasuring = await WriteToCharacteristic(CP_START_REALTIME_STEPS, UUID_CH_CONTROL_POINT, _mainService);

                return suscribed && startedMeasuring;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to steps: {ex.Message}");

                return false;
            }
        }

        public async Task<bool> StopMeasuringSteps() {
            bool stoppedMeasuring;

            try {
                if (_mainService == null) _mainService = await Device.GetServiceAsync(UUID_SV_MAIN);

                stoppedMeasuring = await WriteToCharacteristic(CP_STOP_REALTIME_STEPS, UUID_CH_CONTROL_POINT, _mainService);

                return stoppedMeasuring;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error stopping steps measurement: {ex.Message}");

                return false;
            }
        }

        public async Task<bool> StartMeasuringHeartRate() {
            UserProfile userProfile;
            string address;
            bool wroteUserInfo;
            bool suscribed;

            try {
                if (_mainService == null) _mainService = await Device.GetServiceAsync(UUID_SV_MAIN);

                userProfile = new UserProfile(10000000, UserProfile.GENDER_FEMALE, 26, 154, 49, "Rita", 0); // TODO: Use user's data
                address = ((BluetoothDevice) Device.NativeDevice).Address;
                wroteUserInfo = await WriteToCharacteristic(userProfile.toByteArray(address), UUID_CH_USER_INFO, _mainService);
                suscribed = await SubscribeToHeartRate(OnHeartRate);

                return wroteUserInfo && suscribed;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to heart rate: {ex.Message}");

                return false;
            }
        }

        public async Task<bool> StopMeasuringHeartRate() {
            IService service;
            ICharacteristic controlPoint;
            string address;
            bool stoppedMeasuring;

            try {
                service = await Device.GetServiceAsync(UUID_SV_HEART_RATE);
                address = ((BluetoothDevice) Device.NativeDevice).Address;
                controlPoint = await service.GetCharacteristicAsync(UUID_CH_HEART_RATE_CONTROL_POINT);
                stoppedMeasuring = await WriteToCharacteristic(HR_CP_STOP_HEART_RATE_CONTINUOUS, controlPoint);

                return stoppedMeasuring;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error stopping heart rate mesurement: {ex.Message}");

                return false;
            }
        }


        /**************************************************************************

            Event handlers

         **************************************************************************/

        private void OnSteps(object sender, CharacteristicUpdatedEventArgs arguments) {
            byte[] stepsBytes;
            int stepsValue;
            EventHandler<MeasureEventArgs> steps;

            stepsBytes = arguments.Characteristic.Value;
            stepsValue = DecodeSteps(stepsBytes);
            steps = Steps;

            Log.Debug("BAND", $"##### STEPS UPDATED: {stepsValue}");

            if (steps != null) steps(this, new MeasureEventArgs(stepsValue));
        }

        private void OnHeartRate(object sender, CharacteristicUpdatedEventArgs arguments) {
            byte[] heartRateBytes;
            int heartRateValue;
            EventHandler<MeasureEventArgs> heartRate;

            heartRateBytes = arguments.Characteristic.Value;
            heartRateValue = DecodeHeartRate(heartRateBytes);
            heartRate = HeartRate;

            if (heartRateValue > 0) {
                Log.Debug("BAND", $"##### HEART RATE UPDATED: {heartRateValue}");

                if (heartRate != null) heartRate(this, new MeasureEventArgs(heartRateValue));
            }
        }


        /**************************************************************************

            Private methods

         **************************************************************************/

        private async Task<byte[]> ReadFromCharacteristic(ICharacteristic characteristic) {
            Log.Debug("BAND", $"##### ReadFromCharacteristic(characteristic): Trying to get data from characteristic {characteristic.Uuid}...");

            try {
                if (characteristic.CanRead) return await characteristic.ReadAsync();

                Log.Debug("BAND", $"##### Characteristic {characteristic.Uuid} does not support read");

                return null;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error getting characteristic data: {ex.Message}");

                return null;
            }
        }

        private async Task<byte[]> ReadFromCharacteristic(Guid UUIDCharacteristic, IService service) {
            ICharacteristic characteristic;

            Log.Debug("BAND", $"##### ReadFromCharacteristic(UUIDCharacteristic, service): Trying to get data from characteristic {UUIDCharacteristic}...");

            try {
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await ReadFromCharacteristic(characteristic);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error getting characteristic data: {ex.Message}");

                return null;
            }
        }

        private async Task<byte[]> ReadFromCharacteristic(Guid UUIDCharacteristic, Guid UUIDService) {
            ICharacteristic characteristic;
            IService service;

            Log.Debug("BAND", $"##### ReadFromCharacteristic(UUIDCharacteristic, UUIDService): Trying to get data from characteristic {UUIDCharacteristic}...");

            try {
                service = await Device.GetServiceAsync(UUIDService);
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await ReadFromCharacteristic(characteristic);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error getting characteristic data: {ex.Message}");

                return null;
            }
        }

        private async Task<bool> WriteToCharacteristic(byte[] data, ICharacteristic characteristic) {
            Log.Debug("BAND", $"##### WriteToCharacteristic(data, characteristic): Trying to write to characteristic {characteristic.Uuid}...");

            try {
                if (characteristic.CanWrite) return await characteristic.WriteAsync(data);

                Log.Debug("BAND", $"##### Characteristic {characteristic.Uuid} does not support write");

                return false;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error writing to characteristic: {ex.ToString()}");

                return false;
            }
        }

        private async Task<bool> WriteToCharacteristic(byte[] data, Guid UUIDCharacteristic, IService service) {
            ICharacteristic characteristic;

            Log.Debug("BAND", $"##### WriteToCharacteristic(data, UUIDCharacteristic, service): Trying to write to characteristic {UUIDCharacteristic}...");

            try {
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await WriteToCharacteristic(data, characteristic);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error writing to characteristic: {ex.ToString()}");

                return false;
            }
        }

        private async Task<bool> WriteToCharacteristic(byte[] data, Guid UUIDCharacteristic, Guid UUIDService) {
            ICharacteristic characteristic;
            IService service;

            Log.Debug("BAND", $"##### WriteToCharacteristic(data, UUIDCharacteristic, UUIDService): Trying to write to characteristic {UUIDCharacteristic}...");

            try {
                service = await Device.GetServiceAsync(UUIDService);                    
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await WriteToCharacteristic(data, characteristic);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error writing to characteristic: {ex.ToString()}");

                return false;
            }
        }

        private async Task<bool> SubscribeTo(ICharacteristic characteristic, EventHandler<CharacteristicUpdatedEventArgs> callback) {
            Log.Debug("BAND", $"##### SubscribeTo(characteristic, callback): Trying to subscribe to characteristic {characteristic.Uuid}...");

            try {
                if (_eventHandlers.Add(callback)) {
                    characteristic.ValueUpdated += callback;

                    await characteristic.StartUpdatesAsync();
                }

                return true;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to characteristic: {ex.Message}");

                return false;
            }
        }

        private async Task<bool> SubscribeTo(Guid UUIDCharacteristic, IService service, EventHandler<CharacteristicUpdatedEventArgs> callback) {
            ICharacteristic characteristic;

            Log.Debug("BAND", $"##### SubscribeTo(UUIDCharacteristic, service, callback): Trying to subscribe to characteristic {UUIDCharacteristic}...");

            try {
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await SubscribeTo(characteristic, callback);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to characteristic: {ex.Message}");

                return false;
            }
        }

        private async Task<bool> SubscribeTo(Guid UUIDCharacteristic, Guid UUIDService, EventHandler<CharacteristicUpdatedEventArgs> callback) {
            IService service;
            ICharacteristic characteristic;

            Log.Debug("BAND", $"##### SubscribeTo(UUIDCharacteristic, UUIDservice, callback): Trying to subscribe to characteristic {UUIDCharacteristic}...");

            try {
                service = await Device.GetServiceAsync(UUIDService);
                characteristic = await service.GetCharacteristicAsync(UUIDCharacteristic);

                return await SubscribeTo(characteristic, callback);
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to characteristic: {ex.Message}");

                return false;
            }
        }

        private async Task<bool> SubscribeToHeartRate(EventHandler<CharacteristicUpdatedEventArgs> callback) {
            IService service;
            ICharacteristic controlPoint;
            bool suscribed;
            bool stoppedMeasuring;
            bool startedMeasuring;

            try {
                service = await Device.GetServiceAsync(UUID_SV_HEART_RATE);
                controlPoint = await service.GetCharacteristicAsync(UUID_CH_HEART_RATE_CONTROL_POINT);
                suscribed = await SubscribeTo(UUID_CH_HEART_RATE, service, callback);
                stoppedMeasuring = await WriteToCharacteristic(HR_CP_STOP_HEART_RATE_CONTINUOUS, controlPoint);
                startedMeasuring = await WriteToCharacteristic(HR_CP_START_HEART_RATE_CONTINUOUS, controlPoint);

                return suscribed && stoppedMeasuring && startedMeasuring;
            }
            catch (Exception ex) {
                Log.Error("BAND", $"Error subscribing to heart rate: {ex.Message}");

                return false;
            }
        }

        private int DecodeHeartRate(byte[] heartRate) {
            if (heartRate.Count() == 2 && heartRate[0] == 6) return (heartRate[1] & 0xff);

            Log.Debug("BAND", "##### Received invalid heart rate value");

            return (heartRate[0] & 0xff);
        }

        private int DecodeSteps(byte[] steps) {
            return 0xff & steps[0] | (0xff & steps[1]) << 8;
        }
    }

    public class MeasureEventArgs : EventArgs {
        private readonly int _measure;

        public MeasureEventArgs(int measure) {
            _measure = measure;
        }

        public int Measure {
            get { 
                return _measure; 
            }
        }
    }
}