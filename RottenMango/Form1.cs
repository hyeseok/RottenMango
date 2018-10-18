﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using RottenMango.Data;

namespace RottenMango
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        private readonly PerformanceCounter _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter _ram = new PerformanceCounter("Memory", "Available MBytes");

        private readonly PerformanceCounter _ramPersentage =
            new PerformanceCounter("Memory", "% Committed Bytes In Use");

        readonly Process[] _allProc = Process.GetProcesses();

        private bool _isUseable = true;

        // CPU값 받기
        List<string> processNameList = new List<string>();

        private Dictionary<string, PerformanceCounter> PerformanceCounters =
            new Dictionary<string, PerformanceCounter>();

        private int no = 0;

        // 메인 Form
        public Form1()
        {
            InitializeComponent();

            GetCurrentCpuUsage(); //실시간 CPU
            GetAvailableRam(); //실시간 RAM

            timer1.Interval = 500; //0.5초 간격
            timer1.Enabled = true;

            Thread my_thread = new Thread(new ThreadStart(_check_system));
            my_thread.Start();

//            ProcessList();
        }

        //사용중인 CPU
        public void GetCurrentCpuUsage()
        {
            float CpuUse = _cpu.NextValue();
            if (CpuUse >= 90)
                MessageBox.Show("위험!!");
            else if (CpuUse >= 70)
                MessageBox.Show("경고!!");
            UseableCPU.Text = Convert.ToString((int) CpuUse) + "%";
            metroProgressBarCPU.Value = (int) CpuUse;
            processChart.Series["CPU"].Points.AddY(CpuUse);

            metroProgressSpinnerCPU.Value = (int) CpuUse;
        }

        //사용가능한 RAM
        public void GetAvailableRam()
        {
            float RamUse = _ram.NextValue();
            float ramUsePersentage = _ramPersentage.NextValue();
            UseableRAM.Text = Convert.ToString(RamUse) + "MB";
            UseableRAMPer.Text = Convert.ToString((int) ramUsePersentage) + "%";
            metroProgressBarRAM.Value = (int) ramUsePersentage;
            processChart.Series["RAM"].Points.AddY(ramUsePersentage);
        }

        //실시간 처리 (1초마다 변경)
        private void timer1_Tick(object sender, EventArgs e)
        {
            GetCurrentCpuUsage(); //실시간 CPU
            GetAvailableRam(); //실시간 RAM   

//            ProcessList();
        }


        //CPU 실시간 측정 버튼 클릭시 ( 사용 or 멈춤 )  
        private void CheckUseable_Click_1(object sender, EventArgs e)
        {
            if (_isUseable)
            {
                timer1.Enabled = false;
                _isUseable = false;
                CheckUseable.Text = "사용량 측정";
            }
            else
            {
                timer1.Enabled = true;
                _isUseable = true;
                CheckUseable.Text = "사용량 멈춤";
            }

            GetCurrentCpuUsage(); //실시간 CPU
            GetAvailableRam(); //실시간 RAM
        }


        // 현재 CPU 값 받아오기
        private bool isFirst = true;
        public PerformanceCounter process_cpu;
        List<string> list = new List<string>();
        private string cpuName = "";

        //        public void ProcessList()
        //        {
        //            try
        //            {
        //                int num = 0;
        //                if (isFirst)
        //                {
        //                    foreach (Process ap in _allProc)
        //                    {
        //                        // 프로그램 관리 리스트
        //                        process_cpu =
        //                            new PerformanceCounter("Process", "% Processor Time", ap.ProcessName);
        //                        cpuName = ap.ProcessName;
        //
        //                        dataGridView.Rows.Add(
        //                            num++,
        //                            ap.ProcessName,
        //                            ap.WorkingSet64,
        //                            ap.Id,
        //                            ap.VirtualMemorySize64,
        //                            process_cpu.NextValue()
        //                        );
        //                        list.Add(cpuName);
        //                    }
        //
        //                    isFirst = false;
        //                }
        //                else
        //                {
        //                    for (int i = 0; i < dataGridView.RowCount - 1; i++)
        //                    {
        //                        try
        //                        {
        //                            string pidname = dataGridView.Rows[i].Cells["pidName"].Value.ToString();
        ////                            Debug.WriteLine(pidname);
        //                            if (pidname == "WmiApSrv") continue;
        //                            process_cpu =
        //                                new PerformanceCounter("Process", "% Processor Time", pidname);
        ////                            Debug.WriteLine(process_cpu.NextValue());
        //                            Debug.WriteLine(list.Count);
        ////                            dataGridView.Rows[i].Cells["row"].Value = list.IndexOf(i);
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            Debug.WriteLine(e);
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                Debug.WriteLine(e);
        //            }
        //        }

        //DB 생성 (insert)
        ProcessSnapshotData psd = new ProcessSnapshotData();

        private void _check_system()
        {
            do
            {
                List<Process> _processes = Process.GetProcesses().ToList();


                foreach (Process process in _processes)
                {
                    if (!processNameList.Contains(process.ProcessName))
                    {
                        processNameList.Add(process.ProcessName);

                        PerformanceCounters.Add(process.ProcessName,
                            new PerformanceCounter("Process", "% Processor Time", process.ProcessName));


                        Invoke(new Action(delegate()
                        {
                            cpuGridView.Rows.Add(
                                no++,
                                process.ProcessName,
                                PerformanceCounters[process.ProcessName].NextValue()
                            );
                        }));
                    }
                    //dddddddddddddd
//                    psd.Insert(
//                        process.ProcessName,
//                        PerformanceCounters[process.ProcessName].NextValue(),
//                        process.WorkingSet64,
//                        process.StartTime
//                        );
                }

                for (int i = 0; i < cpuGridView.Rows.Count; i++)
                {
                    try
                    {
                        string ProcessName = cpuGridView.Rows[i].Cells["procName"].Value.ToString();
                        cpuGridView.Rows[i].Cells["cpuValue"].Value = PerformanceCounters[ProcessName].NextValue();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            } while (true);
        }

        // 시작프로그램 레지스터리 리스트
        public void StartRegistryList()
        {
            const string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            using (RegistryKey startupKey = Registry.LocalMachine.OpenSubKey(runKey))
            {
                var valueNames = startupKey.GetValueNames();

                // Name => File path
                Dictionary<string, string> appInfos = valueNames
                    .Where(valueName => startupKey.GetValueKind(valueName) == RegistryValueKind.String)
                    .ToDictionary(valueName => valueName, valueName => startupKey.GetValue(valueName).ToString());
            }
        }


        private void metroTabPage1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
//            Debug.WriteLine(process_cpu.NextValue().ToString());
        }
    }
}