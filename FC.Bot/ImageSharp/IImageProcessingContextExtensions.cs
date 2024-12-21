// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC.Bot.ImageSharp
{
	using System;
	using SixLabors.Fonts;
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.Drawing.Processing;
	using SixLabors.ImageSharp.Processing;

	public static class IImageProcessingContextExtensions
	{
		public static void DrawText(this IImageProcessingContext context, TextOptions op, string? text, Font font, Color color, Point bounds)
		{
			Rectangle rectangle = new(bounds, context.GetCurrentSize());
			context.DrawText(op, text, font, color, rectangle);
		}

		public static void DrawText(this IImageProcessingContext context, TextOptions op, string? text, Font font, Color color, PointF bounds)
		{
			Rectangle rectangle = new((Point)bounds, context.GetCurrentSize());
			context.DrawText(op, text, font, color, rectangle);
		}

		public static void DrawText(this IImageProcessingContext context, TextOptions op, string? text, Font font, Color color, Rectangle bounds)
		{
			if (string.IsNullOrEmpty(text))
				return;

			op.Font = font;
			op.WrappingLength = bounds.Width;

			op.Origin = new Point(bounds.X, bounds.Y);

			bool fits = false;
			while (!fits)
			{
				FontRectangle size = TextMeasurer.Measure(text, op);
				fits = size.Height <= bounds.Height && size.Width <= bounds.Width;

				if (!fits)
				{
					text = text.Truncate(text.Length - 5);
				}
			}

			context.DrawText(op, text, color);
		}

		public static void DrawTextAnySize(this IImageProcessingContext context, TextOptions op, string? text, FontFamily font, Color color, Rectangle bounds)
		{
			if (string.IsNullOrEmpty(text))
				return;

			int fontSize = 64;
			bool fits = false;

			op.Origin = new Point(bounds.X, bounds.Y);

			while (!fits)
			{
				op.Font = font.CreateFont(fontSize);
				FontRectangle size = TextMeasurer.Measure(text, op);
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

			context.DrawText(op, text, color);
		}
	}
}
