using System;
using System.Windows;

namespace GeneticKeyboard1
{
    public class KeyboardGene
    {

        //variables to hold sizes of matrices and other constants
        private const int N = 32; //total number of keys that have flow data (digraph probability) available
        public const int LayoutRowCount = 4;//how many rows in the layout
        public const int LayoutColumnCount = 10;//how many columns in the layout
        //private const double Tolerance7 = 0.0000001;//tolerance level of any double comparision in this program
        public const int PopulationSize = 100;//max number of saved genes at a time
        private static readonly Random Random = new Random();//for mutations
        private const int MaxMutations = 2;//max amount of mutations per creation of child will be 4 individual keys out of the total N but we are swapping so divide by 2
        private static readonly double MutationRate = (double)MaxMutations / N;//to use when actually mutating

        //from "ArabicDigraphsFlow.txt" which I extracted from a research paper but for my keyboard, using integers by multiplying original float by 10 to reduce computer instructions
        // since I'm merging both 'أ' and 'إ' to the same key, I have to merge corresponding rows and columns by taking the max value of each and put on the first one (i.e. the 'أ')
        private static readonly int[,] FlowMatrix = new int[N, N]{
            { 0,12,7,10,11,6,8,3,7,6,0,4,7,4,2,5,4,3,0,3,2,2,0,2,0,1,1,1,2,1,1,0 }
            , { 83,5,4,3,7,0,0,2,2,7,0,1,1,2,1,2,1,1,3,1,1,0,0,1,0,2,0,1,1,0,0,1 }
            , { 3,9,1,5,4,6,8,3,6,2,0,6,2,4,13,2,2,3,1,2,1,1,0,1,0,1,2,1,1,1,1,2 }
            , { 8,16,3,1,4,1,1,3,1,2,0,2,3,2,0,2,0,2,2,2,0,1,0,0,0,0,0,1,0,0,0,0 }
            , { 2,4,3,4,0,2,3,3,2,1,0,3,2,2,1,2,2,1,3,1,1,0,0,1,0,0,1,0,0,0,0,0 }
            , { 12,3,9,11,5,0,1,1,3,3,0,1,1,1,0,2,0,0,3,1,1,0,0,0,0,0,0,0,0,0,0,0 }
            , { 8,2,6,3,5,0,0,3,4,3,0,2,1,1,2,2,2,2,1,1,1,2,0,1,0,1,0,1,0,0,1,0 }
            , { 9,7,3,2,2,3,1,1,1,1,0,0,0,4,1,2,1,2,0,0,0,0,0,0,0,1,0,0,0,0,0,0 }
            , { 4,4,2,1,2,1,3,2,0,2,0,0,0,2,0,1,1,0,1,1,1,0,0,1,0,0,0,0,0,0,0,0 }
            , { 3,6,2,4,2,0,1,2,3,0,0,0,0,1,1,0,1,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0 }
            , { 1,3,13,2,0,2,4,0,1,2,0,3,0,1,1,1,1,1,0,1,0,0,0,1,0,0,0,0,0,0,0,0 }
            , { 5,3,4,3,2,2,1,1,2,3,0,1,1,0,0,0,3,3,0,1,1,0,0,0,0,1,0,0,0,0,0,0 }
            , { 2,4,3,1,1,3,1,3,2,1,0,1,0,1,1,0,0,0,1,1,0,1,0,0,0,0,0,0,1,0,0,0 }
            , { 4,4,3,3,2,2,2,1,1,0,0,0,0,0,1,1,0,1,2,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 2,3,1,0,2,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0 }
            , { 2,5,2,1,1,0,2,1,1,0,0,0,0,1,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0 }
            , { 2,4,2,1,2,1,1,2,1,1,0,0,0,0,1,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0,0,0 }
            , { 2,4,1,2,1,0,1,2,1,0,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0 }
            , { 0,6,0,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 2,3,1,2,2,1,2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,2,0,1,1,1,0,1,0,0,0,0,0,0,0,0,1,0,1,0,0,0,0,0,0,1,0,0,0,0,0,0 }
            , { 1,3,1,1,0,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,0,1,1,0,1,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0 }
            , { 1,1,1,0,1,1,0,1,1,0,0,0,0,1,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0 }
            , { 0,7,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,2,1,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,1,1,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 0,2,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 1,0,1,0,1,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
            , { 3,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 } };

        //from "ArabicLettersArangedByProbability.txt" the probability of each key which I also extracted from a research paper but for my keyboard
        public static readonly char[] KeyArrangedByProbabilityArray = new char[N]{
            'ا', 'ل', 'ي', 'م', 'و', 'ن', 'ر', 'ت', 'ب', 'ع', 'ة', 'د', 'ه', 'س', 'ف', 'ك', 'ق', 'ح', 'أ', 'ج', 'ص', 'ش', 'ط', 'ى', 'خ', 'ز', 'ث', 'ذ', 'ض', 'غ', 'ء' , 'ظ'};

        //from "ArabicLetterProbability.txt" the probability of each key which I also extracted from a research paper but for my keyboard
        private static readonly double[] KeyProbabilityArray = new double[N]{
            14.8,11.4,8,6.4,5.6,5.1,4.7,4.1,3.8,3.4,3.3,3.1,2.7,2.6,2.5,2.1,2.1,1.8,2.6,1.5,1,1,1,0.9,0.9,0.6,0.6,0.6,0.5,0.4,0.3,0.2 };

        //from "Seed1.txt" which I constructed in Graduation Project 1
        public static readonly char[,] SeedLayoutMatrix = new char[LayoutRowCount, LayoutColumnCount]{
            { 'ط', 'ة', 'ت', 'ي', 'ل', 'ا', 'و', 'ر', 'ه', 'b' }
            , { 'ظ', 'س', 'ع', 'ن', 's', 's', 'م', 'ب', 'د', 'أ' }
            , { '.', 'ش', 'ص', 'ء', 'ج', 'ح', 'ك', 'ق', 'ف', 'ؤ' }
            , { 'g', '1', 'غ', 'ض', 'ث', 'ذ', 'خ', 'ى', 'ز', 'e' } };

        //almost like the current most used layout in order to introduce little change (acceptance of reality)
        public static readonly char[,] CurrentUsedLayoutMatrix = new char[LayoutRowCount, LayoutColumnCount]{
            { 'ص', 'ث', 'ق', 'ف', 'غ', 'ع', 'ه', 'خ', 'ح', 'b' }
            , { 'س', 'ي', 'ب', 'ل', 's', 's', 'ن', 'م', 'ك', 'ط' }
            , { '.', 'ء', 'ؤ', 'ر', 'ى', 'ة', 'و', 'ز', 'ظ', 'د' }
            , { 'g', '1', 'ض', 'ش', 'ذ', 'أ', 'ا', 'ت', 'ج', 'e' } };

        //output matrix, but initialized with an alphabet order for testing purposes
        public static char[,] ResultLayoutMatrix = new char[LayoutRowCount, LayoutColumnCount]{
            { 'ح', 'ج', 'ث', 'ة', 'ت', 'ب', 'ء', 'أ', 'ا', 'b' }
            , { 'ص', 'ش', 'س', 'ز', 's', 's', 'ر', 'ذ', 'د', 'خ' }
            , { '.', 'ل', 'ك', 'ق', 'ف', 'غ', 'ع', 'ظ', 'ط', 'ض' }
            , { 'g', '1', 'ي', 'ى', 'ؤ', 'و', 'ه', 'ن', 'م', 'e' } };

        //and a vial to hold minimum fitness for this run
        public static double MinFitness = double.MaxValue;//initializes to max to make sure min is found after processing

        //holder of this gene layout of the keyboard
        public char[,] layoutMatrix = new char[LayoutRowCount, LayoutColumnCount];

        //holder of this gene Distance Matrix
        private double[,] distanceMatrix = new double[N, N];

        //holder of fitness value for this gene
        public double Fitness = double.MaxValue;//initializes to max to make sure min is found after processing

        //the evaluation function
        public void ComputeFitness()
        {
            //first we need to know the distance matrix for this layout
            ComputeDistances();

            double timeSum;
            Fitness = 0;//reset total
            // then is the time sum of product of flow by log2 distance+1 for each key as in the used formula
            // using j because the flow matrix is using columns as starter of the digraph
            for (int j = 0; j < N; j++)
            {
                timeSum = 0;
                for (int i = 0; i < N; i++)
                {
                    timeSum += FlowMatrix[i, j] * Math.Log((distanceMatrix[i, j] + 1), 2);
                }

                // then using the actual usage ratio of character keys each by that time sum
                Fitness += KeyProbabilityArray[j] * timeSum;
            }
        }

        //the creator of comparable distances matrix
        private void ComputeDistances()
        {
            // using formula to calculate distances matrix for the gene but in least calculations just to make compare viable
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    //same key has zero distance
                    if (i == j)
                    {
                        distanceMatrix[i, j] = 0;
                    }
                    else //different keys distances but for compare we don't need to find square root (the exact value) to reduce computer instructions
                    {
                        Point firstKeyPosition = FindLetterPosition(KeyArrangedByProbabilityArray[i]);
                        Point secondKeyPosition = FindLetterPosition(KeyArrangedByProbabilityArray[j]);

                        distanceMatrix[i, j] = (secondKeyPosition - firstKeyPosition).LengthSquared;
                    }
                }
            }
        }

        //helper method to get position of given letter in the layout of this gene
        private Point FindLetterPosition(char letter)
        {
            // search for position of key letter inside this layout
            for (int i = 0; i < LayoutRowCount; i++)
            {
                for (int j = 0; j < LayoutColumnCount; j++)
                {
                    //is it the key we want
                    if (letter == layoutMatrix[i, j])
                    {
                        return new Point(i, j);
                    }
                }
            }
            return new Point(0, 0);//default if not found
        }

        //this mutates current gene when called
        public void Mutate()
        {
            int delta = 0;//to keep track of the number of mutations per child
            char temp;//to hold value while swapping
            Point firstSwapPosition;//position of key to swap
            Point secondSwapPosition;//and the one to swap with

            foreach (var key in KeyArrangedByProbabilityArray)//because we only want to change these keys, the others are fixed, for example space and backspace are fixed
            {
                if (delta < MaxMutations)
                {
                    //mutate at random
                    if (Random.NextDouble() < MutationRate)
                    {
                        //find swap key in this gene
                        firstSwapPosition = FindLetterPosition(key);

                        //choose key to swap with at random and get its place in this gene
                        secondSwapPosition = FindLetterPosition(chooseOtherKeyRandomly(key));

                        //do the swapping
                        temp = key;
                        layoutMatrix[(int)firstSwapPosition.X, (int)firstSwapPosition.Y] = layoutMatrix[(int)secondSwapPosition.X, (int)secondSwapPosition.Y];
                        layoutMatrix[(int)secondSwapPosition.X, (int)secondSwapPosition.Y] = temp;

                        //add one to delta because we mutated one key position
                        delta += 1;
                    }
                }
            }
        }

        //helper method to choose second key of swap and make sure it's a viable choice
        private char chooseOtherKeyRandomly(char originalKey)
        {
            //initialize as same to condition the upcoming loop
            char otherKey = originalKey;

            //make sure they are different before exiting the method
            while (otherKey == originalKey)
            {
                //choose viable other randomly
                int randomPositionOfViableOther = Random.Next(KeyArrangedByProbabilityArray.Length);
                otherKey = KeyArrangedByProbabilityArray[randomPositionOfViableOther];
            }
            return otherKey;
        }

        //constructor
        public KeyboardGene(char[,] baseLayout)
        {
            for (int i = 0; i < LayoutRowCount; i++)
            {
                for (int j = 0; j < LayoutColumnCount; j++)
                {
                    layoutMatrix[i, j] = baseLayout[i, j];
                }
            }
        }
    }
}
