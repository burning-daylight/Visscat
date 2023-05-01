using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace ScatteringDiagrams
{
    public partial class mainForm : Form
    {
        bool isFirstLoad = true;
        MieSolution mie;
        HenyeyGreensteinSolution hg;
        ScattDiag[] points = null;
        RegistryService regSrv;

        public mainForm()
        {
            InitializeComponent();
        }

        private void bCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //mie.AnglesAmount = 360;
                points = mie.CalculateScatteringDiagram();
                sw.Stop();

                DrawScattDiag();

                PrintRes();                
            }
            catch (Exception ex)
            {
                MessageDialog.ShowErrorMessage(ex.Message);
            }
        }

        private void DrawScattDiag()
        {
            if (points != null)
            {
                ClearChart(chartIntensity);
                ClearChart(chartLOGIntens);
                ClearChart(chartScattDiag);
                ClearChart(chartScattDiagLOG);
                switch (cbPolarization.SelectedIndex)
                {
                    case 0:
                        double sum = 0;
                        for (int i = 0; i < points.Length; i++)
                        {
                            sum += points[i].intensNatural_norm;
                            chartIntensity.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural_norm);
                            //chartIntensity.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural);
                            chartLOGIntens.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural_LogNorm);

                            if (points[i].angle >= 0 && points[i].angle <= 180)
                            {
                                chartScattDiag.Series["SerieNatural"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensNatural_norm);
                                chartScattDiagLOG.Series["SerieNatural"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensNatural_LogNorm);
                            }
                        }
                        //MessageBox.Show(sum.ToString());
                        break;
                    case 1:
                        for (int i = 0; i < points.Length; i++)
                        {
                            chartIntensity.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen_norm);
                            //chartIntensity.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen);
                            chartLOGIntens.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen_LogNorm);

                            if (points[i].angle >= 0 && points[i].angle <= 180)
                            {
                                chartScattDiag.Series["SerieOrtogonal"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensPerpen_norm);
                                chartScattDiagLOG.Series["SerieOrtogonal"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensPerpen_LogNorm);
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < points.Length; i++)
                        {
                            chartIntensity.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel_norm);
                            //chartIntensity.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel);
                            chartLOGIntens.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel_LogNorm);

                            if (points[i].angle >= 0 && points[i].angle <= 180)
                            {
                                chartScattDiag.Series["SerieParallel"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensParallel_norm);
                                chartScattDiagLOG.Series["SerieParallel"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensParallel_LogNorm);
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < points.Length; i++)
                        {
                            chartIntensity.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural_norm);
                            chartIntensity.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen_norm);
                            chartIntensity.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel_norm);
                            //chartIntensity.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural);
                            //chartIntensity.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen);
                            //chartIntensity.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel);

                            chartLOGIntens.Series["SerieNatural"].Points.AddXY(points[i].angle, points[i].intensNatural_LogNorm);
                            chartLOGIntens.Series["SerieOrtogonal"].Points.AddXY(points[i].angle, points[i].intensPerpen_LogNorm);
                            chartLOGIntens.Series["SerieParallel"].Points.AddXY(points[i].angle, points[i].intensParallel_LogNorm);

                            if (points[i].angle >= 0 && points[i].angle <= 180)
                            {
                                chartScattDiag.Series["SerieNatural"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensNatural_norm);
                                chartScattDiagLOG.Series["SerieNatural"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensNatural_LogNorm);
                                chartScattDiag.Series["SerieOrtogonal"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensPerpen_norm);
                                chartScattDiagLOG.Series["SerieOrtogonal"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensPerpen_LogNorm);
                                chartScattDiag.Series["SerieParallel"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensParallel_norm);
                                chartScattDiagLOG.Series["SerieParallel"].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intensParallel_LogNorm);
                            }
                        }
                        break;
                }
            }
        }

        private void PrintRes()
        {
            tbResults.Text = "";
            tbResults.Text += String.Format("Scattering coeff = {0} mm^-1\r\nAnisotropy factor = {1}\r\nSize parameter = {2}\r\nScattering efficiency = {3}",
                mie.ScatteringCoeff * 1000, Math.Round(mie.AnisotropyFactor, 6),
                Math.Round(mie.SizeParameter, 3),
                Math.Round(mie.ScatteringEfficiency, 3));
        }
        
        private void ClearChart(Chart chart)
        {
            chart.Series[0].Points.Clear();
            chart.Series[1].Points.Clear();
            chart.Series[2].Points.Clear();
            
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            mie = new MieSolution();
            hg = new HenyeyGreensteinSolution();
            regSrv = new RegistryService("ScatteringDiagrams");
            LoadDataFromRegistry();

            chartIntensHG.ChartAreas[0].AxisX.Interval = 15;
            chartIntensity.ChartAreas[0].AxisX.Interval = 15;
            cbPolarization.SelectedIndex = 0;
            if (isFirstLoad)
            {
                SetBinding();
                isFirstLoad = false;
            }

        }

        private void SetBinding()
        {
            numRadius.DataBindings.Add("Value", mie, "ParticleRadius");
            numRefrIndMedium.DataBindings.Add("Value", mie, "RefractiveIndexMedium");
            numRefrIndParticle.DataBindings.Add("Value", mie, "RefractiveIndexParticleReal");
            numWavelength.DataBindings.Add("Value", mie, "Wavelength");
            numConcentration.DataBindings.Add("Value", mie, "Concentration");

            numAnisotropyFactor.DataBindings.Add("Value", hg, "AnisotropyFactor");
        }

        private void cbPolarization_SelectedIndexChanged(object sender, EventArgs e)
        {
            DrawScattDiag();
        }

        private void bCalculateHG_Click(object sender, EventArgs e)
        {
            hg.AnglesAmount = 360;
            PhaseFunction[] points = hg.CalculateHGScatteringDiagram();
            PhaseFunction[] pointsMod = hg.CalculateHGModifiedScatteringDiagram();

            chartIntensHG.Series[0].Points.Clear();
            chartIntensHGMod.Series[0].Points.Clear();
            chartHG.Series[0].Points.Clear();
            chartHGmodif.Series[0].Points.Clear();

            for (int i = 0; i < points.Length; i++)
            {
                chartIntensHG.Series[0].Points.AddXY(points[i].angle, points[i].intens);
                chartIntensHGMod.Series[0].Points.AddXY(pointsMod[i].angle, pointsMod[i].intens);

                if (points[i].angle >= 0 && points[i].angle <= 180)
                {
                    chartHG.Series[0].Points.AddXY(Math.Round(points[i].angle, 1), points[i].intens);
                    chartHGmodif.Series[0].Points.AddXY(Math.Round(pointsMod[i].angle, 1), pointsMod[i].intens);
                }
            }
        }

        #region Registry Load/Save
        public void SaveDataToRegistry()
        {
            string SolutionKey = "Solutions";
            regSrv.SetValue("ParticleRadius", mie.ParticleRadius, SolutionKey);
            regSrv.SetValue("RefractiveIndexMedium", mie.RefractiveIndexMedium, SolutionKey);
            regSrv.SetValue("RefractiveIndexParticleReal", mie.RefractiveIndexParticleReal, SolutionKey);
            regSrv.SetValue("Wavelength", mie.Wavelength, SolutionKey);
            regSrv.SetValue("Concentration", mie.Concentration, SolutionKey);
            regSrv.SetValue("AnisotropyFactor", hg.AnisotropyFactor, SolutionKey);
        }

        private void LoadDataFromRegistry()
        {
            string SolutionKey = "Solutions";
            mie.ParticleRadius = regSrv.GetDoubleValue("ParticleRadius", SolutionKey);
            mie.RefractiveIndexMedium = regSrv.GetDoubleValue("RefractiveIndexMedium", SolutionKey);
            mie.RefractiveIndexParticleReal = regSrv.GetDoubleValue("RefractiveIndexParticleReal", SolutionKey);
            mie.Wavelength = regSrv.GetDoubleValue("Wavelength", SolutionKey);
            mie.Concentration = regSrv.GetDoubleValue("Concentration", SolutionKey);
            hg.AnisotropyFactor = regSrv.GetDoubleValue("AnisotropyFactor", SolutionKey);
        }
        #endregion

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveDataToRegistry();
        }

        private void bExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pbShowBig_Click(object sender, EventArgs e)
        {
            chartLOGIntens.SaveImage("tmp.bmp", ChartImageFormat.Bmp);
            System.Diagnostics.Process.Start(Application.StartupPath + "\\tmp.bmp");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
