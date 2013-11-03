﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace rfdp.Models
{
    public class SigProc
    {
        public string[] datafile { get; set; }
        public double fs { get; set; }
        public double[] mean { get; set; }
        public double[] rms { get; set; }
        public double[] pwr { get; set; }
        public double[] max { get; set; }
        public double[] min { get; set; }
        public string msg { get; set; }
        public int[] len { get; set; }
        public double duration { get; set; }
        public string[] jasondata { get; set; }

        public SigProc()
        {
            init();
            datafile = new string[2] { "", "" };
        }

        private void calc(int chan, double data)
        {
            len[chan]++;
            mean[chan] += data;
            max[chan] = max[chan] > data ? max[chan] : data;
            min[chan] = min[chan] < data ? min[chan] : data;
            rms[chan] += data * data;
            pwr[chan] = rms[chan];
            jasondata[chan] = (len[chan] == 1) ? "" : jasondata[chan] + ", ";
            jasondata[chan] += "{\"x\": " + Convert.ToString(fs * len[chan]) + ", \"y\": " + Convert.ToString(data) + "}";
            
        }

        private void update(int chan)
        {
            if (len[chan] != 0)
            {
                mean[chan] /= len[chan];
                rms[chan] = Math.Sqrt(rms[chan] / len[chan]);
                pwr[chan] /= len[chan];
                duration = len[chan] / fs;
                jasondata[chan] = "[ " + jasondata[chan] + " ]";
            }
        }

        private void init()
        {
            mean = new double[2] {0, 0};
            rms = new double[2] { 0, 0 };
            pwr = new double[2] { 0, 0 };
            max = new double[2] { 0, 0 };
            min = new double[2] { 0, 0 };
            msg = "";
            len = new int[2] {0, 0};
            jasondata = new string[2] {"", ""};
        }


        public void ProcessStat()
        {
            init();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    string appdata = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Data") + "\\" + datafile[i];
                    using (StreamReader sr = new StreamReader(appdata))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line == string.Empty) { continue; }
                            double data = Convert.ToDouble(line);
                            calc(i, data);
                        }
                        update(i);
                    }
                }
                catch (Exception e)
                {
                    Debug.Write("The file could not be read:");
                    Debug.Write(e.Message);
                }
                
            }
            

        }

    }


}