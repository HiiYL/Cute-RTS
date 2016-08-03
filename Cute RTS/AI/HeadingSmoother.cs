using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Cute_RTS.AI
{
    public class HeadingSmoother
    {
        private readonly int numSamples;
        private readonly Stack<Vector2> samples;

        public HeadingSmoother(int numSamples)
        {
            this.numSamples = numSamples;
            samples = new Stack<Vector2>(numSamples);
        }

        public void InsertSample(Vector2 sample)
        {
            samples.Push(sample);
        }

        public Vector2 CalculateSmoothedHeading()
        {
            Vector2 smoothedHeading = Vector2.Zero;
            int samplesMade = 0;
            Stack<Vector2> tempStack = new Stack<Vector2>();

            for (int i = 0; i < numSamples; i++)
            {
                if (samples.Count > 0)
                {
                    Vector2 sample = samples.Pop();
                    smoothedHeading += sample;
                    tempStack.Push(sample);

                    samplesMade++;
                }
                else
                    i = numSamples;
            }

            if (tempStack.Count == numSamples)
                tempStack.Pop();

            while (tempStack.Count > 0)
                samples.Push(tempStack.Pop());

            if (samplesMade == 0)
                return Vector2.Normalize(smoothedHeading);

            return Vector2.Normalize(smoothedHeading / samplesMade);
        }
    }
}
