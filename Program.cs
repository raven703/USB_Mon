using System;
using System.IO;
using System.Collections.Generic;
using System.Management; // need to add System.Management to your project references.



namespace App1
{
    class Program
    {
        private static void VolumeEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("Volume mount. Disk inserted.");
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    Console.WriteLine(string.Format("({0}) {1}", drive.Name.Replace("\\", ""), drive.VolumeLabel));
                }
            }

            ManagementObjectSearcher theSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            foreach (ManagementObject currentObject in theSearcher.Get())
            {
                ManagementObject theSerialNumberObjectQuery = new ManagementObject("Win32_PhysicalMedia.Tag='" + currentObject["DeviceID"] + "'");
                string serial_number = theSerialNumberObjectQuery["SerialNumber"].ToString();
                Console.WriteLine("Serial number: " + serial_number);

              


            }

        }
        private static void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            _ = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            Console.WriteLine("DeviceInsertedEvent");
            //foreach (var property in instance.Properties)
           // {
           //     Console.WriteLine(property.Name + " = " + property.Value);
           // }
        }

        private static void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            _ = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            Console.WriteLine("DeviceRemovedEvent");
            // foreach (var property in instance.Properties)
            //{
            //      Console.WriteLine(property.Name + " = " + property.Value);
            //  }
        }

        static void Main(string[] args)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();

            WqlEventQuery volumeQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            ManagementEventWatcher volumeWatcher = new ManagementEventWatcher(volumeQuery);
            volumeWatcher.EventArrived += new EventArrivedEventHandler(VolumeEvent);
            volumeWatcher.Start();

            // Do something while waiting for events
            System.Threading.Thread.Sleep(20000000);
        }
    }
}

