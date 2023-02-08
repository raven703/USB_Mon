using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Management; // need to add System.Management to your project references.
using System.Net;



namespace App1
{
    class Program
    {
        private static void VolumeEvent(object sender, EventArrivedEventArgs e)
        {
            string time = DateTime.Now.ToString("dd-MM-yyyy HH:mm tt");

            int myEventType = Convert.ToInt32(e.NewEvent.Properties["EventType"].Value.ToString());
            // string DriveLetter = e.NewEvent.Properties["DriveName"].Value.ToString();
            // Console.WriteLine("DL " + DriveLetter);
            switch (myEventType)
            {
                case 2:
                    Console.WriteLine("2: Device arrival");
                    
                    //read file with access serials
                    string[] access_list = File.ReadAllLines(@"\\casper\omnishare\accesslog\access.lst");
                    // get drive letter, serial number from Object
                    ManagementObjectSearcher theSearcher = new ManagementObjectSearcher
                        ("SELECT * FROM Win32_Volume WHERE DriveType='2'");
                    int driveCount = theSearcher.Get().Count;
                    
                    if (driveCount > 0) //check if any removable disk present after query
                    {
                        foreach (ManagementObject currentObject in theSearcher.Get())
                        {

                            string diskSerialNumber = currentObject["SerialNumber"].ToString();
                            string diskDriveLetter = currentObject["DriveLetter"].ToString();
                            string hostName = Dns.GetHostName();
                           

                            if (!access_list.Contains(diskSerialNumber))
                            {
                                Console.WriteLine("Serial Number NOT access List");
                                Console.WriteLine("Disk Letter: " + diskDriveLetter);
                                Console.WriteLine("Serial number: " + diskSerialNumber);
                                Console.WriteLine("Host Name: " + hostName);
                                Console.WriteLine("Time: " + time);

                            }
                         
                            else
                            {
                                Console.WriteLine("Serial Number IN access List");
                            }
                        }
                    }
                    else if (driveCount == 0)
                    {
                        Console.WriteLine("Unknown USB Device.");

                        ManagementObjectSearcher theSearcher2 = new ManagementObjectSearcher(
                            "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
                        foreach (ManagementObject currentObject in theSearcher2.Get())
                        {
                           
                            string serialNumber = currentObject["SerialNumber"].ToString();
                            string model = currentObject["Model"].ToString();
                            string systemName = currentObject["SystemName"].ToString();
                            Console.WriteLine("Serial number: " + serialNumber);
                            Console.WriteLine("Model: " + model);
                            Console.WriteLine("System Name: " + systemName);
                            Console.WriteLine("Time: " + time);
                        }


                    }
                    break;
                case 3:
                    Console.WriteLine("3: Device removal");
                    break;

            }



            //Console.WriteLine("Event type: " + e.NewEvent.Properties["EventType"].Value);
            //Console.WriteLine("Drive letter: " + e.NewEvent.Properties["DriveName"].Value);

            

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
            WqlEventQuery volumeQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent "); //WHERE EventType = 2
            ManagementEventWatcher volumeWatcher = new ManagementEventWatcher(volumeQuery);
            volumeWatcher.EventArrived += new EventArrivedEventHandler(VolumeEvent);
            volumeWatcher.Start();
            
            
            // WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");

            // ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            // insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            // insertWatcher.Start();

            // WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            // ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            // removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            // removeWatcher.Start();

            

            // Do something while waiting for events
            System.Threading.Thread.Sleep(20000000);
        }
    }
}

