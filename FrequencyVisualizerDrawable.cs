using Microsoft.Maui.Graphics;



public class FrequencyVisualizerDrawable : IDrawable
{
    private readonly double[] _frequencies;

    public FrequencyVisualizerDrawable(double[] frequencies)
    {
        _frequencies = frequencies;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.Blue;

        float barWidth = (float)(dirtyRect.Width / _frequencies.Length);
        for (int i = 0; i < _frequencies.Length; i++)
        {
            float barHeight = (float)(_frequencies[i] / 10 * dirtyRect.Height);
            canvas.FillRectangle(i * barWidth, dirtyRect.Height - barHeight, barWidth - 2, barHeight);
        }
    }
}
