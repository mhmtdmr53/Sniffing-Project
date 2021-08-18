using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SharpPcap.LibPcap;
using SharpPcap;
using PacketDotNet;


namespace pcapTest
{
    public partial class MainForm : Form
    {
        List<LibPcapLiveDevice> interfaceList = new List<LibPcapLiveDevice>();
        int selectedIntIndex;
        LibPcapLiveDevice wifi_device;
        CaptureFileWriterDevice captureFileWriter;
        Dictionary<int, Packet> capturedPackets_list = new Dictionary<int, Packet>();

        int packetNumber = 1;
        string time_str = "", sourceIP = "", destinationIP = "", protocol_type = "", length = "";

        bool YenidenDinleme = false;

        Thread sniffing;

        public MainForm(List<LibPcapLiveDevice> interfaces, int selectedIndex)
        {
            InitializeComponent();
            this.interfaceList = interfaces;
            selectedIntIndex = selectedIndex;
          
            wifi_device = interfaceList[selectedIntIndex];
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)// Dinleme Başlatılır
        {
            if(YenidenDinleme == false) //ilk seferde
            {
                System.IO.File.Delete(Environment.CurrentDirectory + "dosya.pcap");
                wifi_device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                sniffing = new Thread(new ThreadStart(sniffing_Proccess));
                sniffing.Start();
                toolStripButton1.Enabled = false;
                toolStripButton2.Enabled = true;
                

            }
           else if (YenidenDinleme)
            {
                if (MessageBox.Show("Paketler dosyaya yazıldı", "Confirm", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    
                    System.IO.File.Delete(Environment.CurrentDirectory + "dosya.pcap");
                    listView1.Items.Clear();
                    capturedPackets_list.Clear();
                    packetNumber = 1;
                    textBox2.Text = "";
                    wifi_device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);
                    sniffing = new Thread(new ThreadStart(sniffing_Proccess));
                    sniffing.Start();
                    toolStripButton1.Enabled = false;
                    toolStripButton2.Enabled = true;
                    
                }
            }
            YenidenDinleme = true;
        }

        // paket bilgisi
        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string protocol = e.Item.SubItems[4].Text;
            int key = Int32.Parse(e.Item.SubItems[0].Text);
            Packet packet;
            bool getPacket  = capturedPackets_list.TryGetValue(key, out packet);

                switch (protocol) {
                case "TCP":
                    if(getPacket)
                    {
                        var tcpPacket = (TcpPacket)packet.Extract(typeof(TcpPacket));
                        if (tcpPacket != null)
                        {
                            int srcPort = tcpPacket.SourcePort;
                            int dstPort = tcpPacket.DestinationPort;
                            var checksum = tcpPacket.Checksum;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: TCP" +
                                            "\r\nSource port:" + srcPort +
                                            "\r\nDestination port: " + dstPort +
                                            "\r\nTCP header size: " + tcpPacket.DataOffset +
                                            "\r\nWindow size: " + tcpPacket.WindowSize + 
                                            "\r\nChecksum:" + checksum.ToString() + (tcpPacket.ValidChecksum ? ",valid" : ",invalid") +
                                            "\r\nTCP checksum: " + (tcpPacket.ValidTCPChecksum ? ",valid" : ",invalid") +
                                            "\r\nSequence number: " + tcpPacket.SequenceNumber.ToString() +
                                            "\r\nAcknowledgment number: " + tcpPacket.AcknowledgmentNumber + (tcpPacket.Ack ? ",valid" : ",invalid") +
                                            
                                            "\r\nUrgent pointer: " + (tcpPacket.Urg ? "valid" : "invalid") +
                                            "\r\nACK flag: " + (tcpPacket.Ack ? "1" : "0") + 
                                            "\r\nPSH flag: " + (tcpPacket.Psh ? "1" : "0") + 
                                            "\r\nRST flag: " + (tcpPacket.Rst ? "1" : "0") + 
                                            
                                            "\r\nSYN flag: " + (tcpPacket.Syn ? "1" : "0") +
                                          
                                           
                                            "\r\nFIN flag: " + (tcpPacket.Fin ? "1" : "0") + 
                                            "\r\nECN flag: " + (tcpPacket.ECN ? "1" : "0") +
                                            "\r\nCWR flag: " + (tcpPacket.CWR ? "1" : "0") +
                                            "\r\nNS flag: " + (tcpPacket.NS ? "1" : "0");
                        }
                    }
                    break;
                case "UDP":
                    if (getPacket)
                    {
                        var udpPacket = (UdpPacket)packet.Extract(typeof(UdpPacket));
                        if (udpPacket != null)
                        {
                            int srcPort = udpPacket.SourcePort;
                            int dstPort = udpPacket.DestinationPort;
                            var checksum = udpPacket.Checksum;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: UDP" +
                                            "\r\nSource port:" + srcPort +
                                            "\r\nDestination port: " + dstPort +
                                            "\r\nChecksum:" + checksum.ToString() + " valid: " + udpPacket.ValidChecksum +
                                            "\r\nValid UDP checksum: " + udpPacket.ValidUDPChecksum;
                        }
                    }
                    break;
                case "ARP":
                    if (getPacket)
                    {
                        var arpPacket = (ARPPacket)packet.Extract(typeof(ARPPacket));
                        if (arpPacket != null)
                        {
                            System.Net.IPAddress senderAddress = arpPacket.SenderProtocolAddress;
                            System.Net.IPAddress targerAddress = arpPacket.TargetProtocolAddress;
                            System.Net.NetworkInformation.PhysicalAddress senderHardwareAddress = arpPacket.SenderHardwareAddress;
                            System.Net.NetworkInformation.PhysicalAddress targerHardwareAddress = arpPacket.TargetHardwareAddress;

                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: ARP" +
                                            "\r\nHardware address length:" + arpPacket.HardwareAddressLength +
                                            "\r\nProtocol address length: " + arpPacket.ProtocolAddressLength +
                                            "\r\nOperation: " + arpPacket.Operation.ToString() + 
                                            "\r\nSender protocol address: " + senderAddress +
                                            "\r\nTarget protocol address: " + targerAddress +
                                            "\r\nSender hardware address: " + senderHardwareAddress +
                                            "\r\nTarget hardware address: " + targerHardwareAddress;
                        }
                    }
                    break;
                case "ICMP":
                    if (getPacket)
                    {
                        var icmpPacket = (ICMPv4Packet)packet.Extract(typeof(ICMPv4Packet));
                        if (icmpPacket   != null)
                        {                
                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: ICMP v4" +
                                            "\r\nType Code: 0x" + icmpPacket.TypeCode.ToString("x") +
                                            "\r\nChecksum: " + icmpPacket.Checksum.ToString("x") +
                                            "\r\nID: 0x" + icmpPacket.ID.ToString("x") +
                                            "\r\nSequence number: " + icmpPacket.Sequence.ToString("x");
                        }
                    }
                    break;
                case "IGMP":
                    if (getPacket)
                    {
                        var igmpPacket = (IGMPv2Packet)packet.Extract(typeof(IGMPv2Packet));
                        if (igmpPacket != null)
                        {
                            textBox2.Text = "";
                            textBox2.Text = "Packet number: " + key +
                                            " Type: IGMP v2" +
                                            "\r\nType: " + igmpPacket.Type +
                                            "\r\nGroup address: " + igmpPacket.GroupAddress +
                                            "\r\nMax response time" + igmpPacket.MaxResponseTime;
                        }
                    }
                    break;
                default:
                    textBox2.Text = "";
                    break;
                }
        }

       

       

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 1)
            {
                int index = listView1.SelectedItems[0].Index;
                listView1.Items[index + 1].Selected = true;
                listView1.Items[index + 1].EnsureVisible();
            }
        }

        private void chooseInterfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Interfaces openInterfaceForm = new Interfaces();
            this.Hide();
            openInterfaceForm.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int index = listView1.SelectedItems[0].Index;
                listView1.Items[index - 1].Selected = true;
                listView1.Items[index - 1].EnsureVisible();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)// Ağ dinleme durdurulmuştur
        {
            sniffing.Abort();
            wifi_device.StopCapture();
            wifi_device.Close();
            captureFileWriter.Close();

            toolStripButton1.Enabled = true;
           
            toolStripButton2.Enabled = false;
        }

        private void sniffing_Proccess()
        {
            
            int readTimeoutMilliseconds = 1000;
            wifi_device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);

           
            if (wifi_device.Opened)
            {
                
                captureFileWriter = new CaptureFileWriterDevice(wifi_device, Environment.CurrentDirectory + "dosya.pcap");
                wifi_device.Capture();
            }
        }

        public void Device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
           
            captureFileWriter.Write(e.Packet);
            

           
            DateTime time = e.Packet.Timeval.Date;
                time_str = (time.Hour + 1 ) + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
                length = e.Packet.Data.Length.ToString();


                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

                // listeye ekler
                capturedPackets_list.Add(packetNumber, packet);

      
            var ipPacket = (IpPacket)packet.Extract(typeof(IpPacket));
               

                if (ipPacket != null)
                {
                    System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                    System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                    protocol_type = ipPacket.Protocol.ToString();
                    sourceIP = srcIp.ToString();
                    destinationIP = dstIp.ToString();



                    var protocolPacket = ipPacket.PayloadPacket;

                    ListViewItem item = new ListViewItem(packetNumber.ToString());
                    item.SubItems.Add(time_str);
                    item.SubItems.Add(sourceIP);
                    item.SubItems.Add(destinationIP);
                    item.SubItems.Add(protocol_type);
                    item.SubItems.Add(length);
                

                    Action action = () => listView1.Items.Add(item);
                    listView1.Invoke(action);
            
                    ++packetNumber;
                }
        }
    }
}


