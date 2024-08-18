﻿namespace StockSharp.Algo.Indicators;

using Ecng.ComponentModel;

/// <summary>
/// The vertical-horizontal filter.
/// </summary>
/// <remarks>
/// https://doc.stocksharp.com/topics/api/indicators/list_of_indicators/vhf.html
/// </remarks>
[Display(
	ResourceType = typeof(LocalizedStrings),
	Name = LocalizedStrings.VHFKey,
	Description = LocalizedStrings.VerticalHorizontalFilterKey)]
[IndicatorIn(typeof(CandleIndicatorValue))]
[Doc("topics/api/indicators/list_of_indicators/vhf.html")]
public class VerticalHorizontalFilter : LengthIndicator<decimal>
{
	// Текущее значение минимума
	private readonly Lowest _min = new();

	// Текущее значение максимума
	private readonly Highest _max = new();

	// Текущее значение суммы
	private readonly Sum _sum = new();

	// Цена закрытия предыдущего бара
	private decimal? _previousClosePrice;

	/// <summary>
	/// Initializes a new instance of the <see cref="VolumeWeightedMovingAverage"/>.
	/// </summary>
	public VerticalHorizontalFilter()
	{
		Length = 15;
	}

	/// <inheritdoc />
	public override IndicatorMeasures Measure => IndicatorMeasures.MinusOnePlusOne;

	/// <inheritdoc />
	protected override bool CalcIsFormed() => _sum.IsFormed && _max.IsFormed && _min.IsFormed;

	/// <inheritdoc />
	public override void Reset()
	{
		_sum.Length = _min.Length = _max.Length = Length;
		_previousClosePrice = null;
		base.Reset();
	}

	/// <inheritdoc />
	protected override IIndicatorValue OnProcess(IIndicatorValue input)
	{
		var candle = input.GetValue<ICandleMessage>();

		// Находим минимум и максимум для заданного периода
		var minValue = _min.Process(input.SetValue(this, candle.LowPrice)).GetValue<decimal>();
		var maxValue = _max.Process(input.SetValue(this, candle.HighPrice)).GetValue<decimal>();

		var sumValue = 0m;

		// Вычисляем сумму модулей разности цен закрытия текущего и предыдущего дня для заданного периода
		if (_previousClosePrice != null)
			sumValue = _sum.Process(input.SetValue(this, Math.Abs(_previousClosePrice.Value - candle.ClosePrice))).GetValue<decimal>();

		if (input.IsFinal)
			_previousClosePrice = candle.ClosePrice;

		if (!IsFormed)
			return new DecimalIndicatorValue(this);

		// Вычисляем значение индикатора
		if (sumValue != 0)
			return new DecimalIndicatorValue(this, ((maxValue - minValue) / sumValue));

		return new DecimalIndicatorValue(this);
	}
}