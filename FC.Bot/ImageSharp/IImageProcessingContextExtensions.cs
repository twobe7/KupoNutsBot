// This document is intended for use by Kupo Nut Brigade developers.

namespace FC.Bot.ImageSharp
{
	using System;
	using SixLabors.Fonts;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.Processing;
	using SixLabors.Primitives;

	public static class IImageProcessingContextExtensions
	{
		public static void DrawText(this IImageProcessingContext context, TextGraphicsOptions op, string? text, Font font, Color color, Rectangle bounds)
		{
			if (string.IsNullOrEmpty(text))
				return;

			op.WrapTextWidth = bounds.Width;

			RendererOptions rOp = new RendererOptions(font);
			rOp.WrappingWidth = bounds.Width;

			bool fits = false;
			while (!fits)
			{
				SizeF size = TextMeasurer.Measure(text, rOp);
				fits = size.Height <= bounds.Height && size.Width <= bounds.Width;

				if (!fits)
				{
					text = text.Truncate(text.Length - 5);
				}
			}

			context.DrawText(op, text, font, color, new Point(bounds.X, bounds.Y));
		}

		public static void DrawTextAnySize(this IImageProcessingContext context, TextGraphicsOptions op, string? text, FontFamily font, Color color, Rectangle bounds)
		{
			if (string.IsNullOrEmpty(text))
				return;

			int fontSize = 64;
			bool fits = false;
			Font currentFont = font.CreateFont(fontSize);
			while (!fits)
			{
				currentFont = font.CreateFont(fontSize);
				SizeF size = TextMeasurer.Measure(text, new RendererOptions(currentFont));
				fits = size.Height <= bounds.Height && size.Width <= bounds.Width;

				if (!fits)
				{
					fontSize -= 2;
				}

				if (fontSize <= 2)
				{
					return;
				}
			}

			context.DrawText(op, text, currentFont, color, new Point(bounds.X, bounds.Y));
		}
	}
}
