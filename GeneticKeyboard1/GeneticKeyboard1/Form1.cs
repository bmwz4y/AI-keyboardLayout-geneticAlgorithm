using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GeneticKeyboard1
{
    public partial class Form1 : Form
    {
        private string startButtonText = "Start computing";
        private string finishButtonText = "Finish computing now";
        private string stopwatchLabelText = "Total time of last run:";
        private string stopwatchInitialText = "hh:mm:ss.ttttttt";
        private string startTimeLabelText = "current run started at:";
        private string startTimeInitialText = "hh:mm:ss";
        private ThreadStart _childref;
        private Thread _childThread;

        // Create new stopwatch.
        private readonly Stopwatch _stopwatch = new Stopwatch();

        //string builder to help in formating output
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public Form1()
        {
            InitializeComponent();

            button1.Text = startButtonText;
            button2.Text = finishButtonText;
            label1.Text = stopwatchLabelText.ToUpper();
            label2.Text = stopwatchInitialText.ToUpper();
            label3.Text = startTimeLabelText.ToUpper();
            label4.Text = startTimeInitialText.ToUpper();

            //before first run only start button can be pressed and the result layout isn't visible at all and make it readonly
            button1.Enabled = true;
            button2.Enabled = false;
            richTextBox1.ReadOnly = true;
            label3.Visible = false;
            label4.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //switch button off and turn the other on
            button1.Enabled = false;
            button2.Enabled = true;

            //disable the Layout richtext if it is visible on consecutive runs and others
            richTextBox1.Enabled = false;
            label1.Enabled = false;
            label2.Enabled = false;
            label3.Enabled = true;
            label4.Enabled = true;

            //set start time as now
            label4.Text = DateTime.Now.ToShortTimeString();

            //show start time
            label3.Visible = true;
            label4.Visible = true;

            // Begin timing.
            _stopwatch.Start();

            //call child thread to start solving without hanging the main program
            _childref = SolveThread;
            _childThread = new Thread(_childref);
            _childThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //switch button off and turn the other on
            button1.Enabled = true;
            button2.Enabled = false;

            //stop computing
            _childThread.Abort();

            //format and show output
            _stringBuilder.Clear();
            for (int i = 0; i < KeyboardGene.LayoutRowCount; i++)
            {
                _stringBuilder.Append("I  ");
                for (int j = 0; j < KeyboardGene.LayoutColumnCount; j++)
                {
                    _stringBuilder.Append(KeyboardGene.ResultLayoutMatrix.GetValue(i, j));
                    //here i used the I english character with spaces between each to keep it left to right so key would be in correct position
                    _stringBuilder.Append("  I  ");
                }
                _stringBuilder.AppendLine();
            }
            _stringBuilder.AppendLine();
            _stringBuilder.Append("The above layout has a fitness value: " + KeyboardGene.MinFitness);
            richTextBox1.Text = _stringBuilder.ToString();

            //show the Layout richtext by using visible and enabled to be able to copy
            richTextBox1.Enabled = true;
            label1.Enabled = true;
            label2.Enabled = true;
            label3.Enabled = false;
            label4.Enabled = false;

            // Stop timing.
            _stopwatch.Stop();

            //show elapsed time since it ran and clear stopwatch
            label2.Text = _stopwatch.Elapsed.ToString("c");
            _stopwatch.Reset();

            //hide start time
            label3.Visible = false;
            label4.Visible = false;
        }

        private static void SolveThread()
        {
            //I'm basically using the current used keyboard layout and my constructed seed as both to be initial father and mother in every run
            KeyboardGene fatherKeyboardGene = new KeyboardGene(KeyboardGene.SeedLayoutMatrix);
            KeyboardGene motherKeyboardGene = new KeyboardGene(KeyboardGene.CurrentUsedLayoutMatrix);

            //This is to keep result from being destroyed which will be used to hold best of each new generation
            KeyboardGene resultKeyboardGene = new KeyboardGene(KeyboardGene.ResultLayoutMatrix);//initialize to default (alphabet)
            resultKeyboardGene.ComputeFitness();//and compute default fitness (alphabet fitness value)
            KeyboardGene.MinFitness = resultKeyboardGene.Fitness;

            try
            {
                //to hold all created solutions dynamically
                KeyboardGene[] population = new KeyboardGene[KeyboardGene.PopulationSize];

                //add seeds
                population[0] = fatherKeyboardGene;
                population[1] = motherKeyboardGene;


                //here is the start actual computing
                //let's find what is the evaluation value for each
                population[0].ComputeFitness();
                population[1].ComputeFitness();

                //start infinite loop stopped only when user calls stopping
                while (true)
                {
                    //start at 2 because of father and mother
                    for (int i = 2; i < KeyboardGene.PopulationSize; i += 2) //each loop creates a generation
                    {
                        //create 2 new children
                        population[i] = new KeyboardGene(population[i - 2].layoutMatrix); //based on its father
                        population[i + 1] = new KeyboardGene(population[i - 1].layoutMatrix); //based on its mother

                        //both to cross over (taking half the keys from father and the other half from mother)
                        //not implemented for now because it'll introduce conflicts and it'll be so time consuming
                        //because for each child I need to loop once to copy and another to read values and make sure all keys are present and if not then copy missing into one placement of any of the duplicate keys
                        //and if I really want cross over to be good I have to get fitness for each key individually and compare father to mother then take the better one

                        //change some keys positions by mutation for both new entries
                        population[i].Mutate();
                        population[i + 1].Mutate();

                        //let's find what is the evaluation value for each
                        population[i].ComputeFitness();
                        population[i + 1].ComputeFitness();
                    }
                    //find which of the population has minimum fitness (least amount of time which is best)
                    foreach (var gene in population)
                    {
                        if (gene.Fitness < KeyboardGene.MinFitness)
                        {
                            resultKeyboardGene = gene;
                            KeyboardGene.MinFitness = gene.Fitness;
                        }
                    }

                    //change worst ancestor after first loop
                    if (population[0].Fitness > population[1].Fitness && population[0].Fitness > resultKeyboardGene.Fitness)
                    {
                        population[0] = resultKeyboardGene;
                    }
                    else if (population[1].Fitness > population[0].Fitness && population[1].Fitness > resultKeyboardGene.Fitness)
                    {
                        population[1] = resultKeyboardGene;
                    }
                }//end of the infinite loop
            }
            finally
            {
                //here it saves the result in the static result matrix
                KeyboardGene.ResultLayoutMatrix = resultKeyboardGene.layoutMatrix;
            }
        }

    }
}
