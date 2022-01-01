# Waveform Generation Libraries

To try out the filter and transform libraries contained within this package you will need waveforms of various types. This simple waveform library gives you a range of waveforms as its output, both periodic waveforms such as sine/cosine waves, and single shot waveforms such as impulses.

The waveform generators are all contained within the `SignalSources` class. Each basic waveform generator function creates a finite sequence of samples that follow a predefined wave shape. Every waveform generator takes two standard parameters, one to set the duration of the sequence as a count of the number of samples generated before the sequence ends. The other standard parameter is a magnitude parameter, usually defaulting to 1.0 if not provided, but which is used to scale the magnitude of each sample that emereges from that waveform generator.

Any remaining parameters depend on the shape of the waveform being generated.

## Sine wave generator

```
public static IEnumerable<double> SineWave
            (int frequency, int phase, int sampleRate, int duration, double magnitude = 1.0)
```
This waveform generator can generate sinewave samples at any phase or frequency. The samples
are returned as an enumerable sequence of double floating point numbers, as is the case with
all the other waveform generators. Parameters are as described below:

| Parameter | Description |
| --- | --- |
| `frequency` | The frequency of the waveform in Hz. This is computed from this parameter and the `sampleRate` parameter. |
| `phase` | The phase of the waveform measured in degrees. For example, a value of 90 for this parameter will yield a cosine wave rather than a sine wave. |
| `sampleRate` | The rate at which samples are generated. The only real purpose of this parameter is to know how many samples per sinewave cycle to generate when combined with the `frequency` parameter. It does not actually force the samples to be generated at this rate.|
| `duration` | The total number of samples to be generated before the IEnumerable<double> sequence ends. |
| `magnitude` | The amplitude of the sine wave generated. This is measured from the zero-valued median value to the peak value of the waveform. |

## Square pulse generator

```
public static IEnumerable<double> Impulse
            (int width, int duration, double magnitude = 1.0)
```
This waveform generator creates a single raised rectangular pulse of duration `width` samples,
followed by `duration - width` samples of zero values. The height of the rectangular pulse is
set by the `magnitude` parameter.

## Raised cosine generator

```
public static IEnumerable<double> RaisedCosine
            (int width, int duration, double magnitude = 1.0)
```
As with the square pulse generator above, this waveform generator generates a single pulse
of duration `width` samples, followed by `duration - width` zero-valued samples. The difference
is that this waveform is in the shape of a single full cycle of a cosine wave. The waveform
starts at a zero value and follows a cosine shape until it reaches its peak value equal to `magnitude` at sample `width/2`, then falls back to zero value in time for sample `width`. Note 
that it is a full raised cosine, meaning the median value for the cosine part of the waveform
is at `magnitude/2` and the waveform traverses from its full negative peak value to full positive peak and back to its negative peak value within the `width` block of samples.

## Synthetic noise generator

```
public static IEnumerable<double> SyntheticNoise(int duration, double magnitude = 1.0)
```
This noise generator creates a sequence of `duration` white noise samples. This is done by
creating a spectrum of `duration` frequencies each with the same amplitude but randomly
distributed phases. The spectrum then has an inverse fast Fourier transform applied to
it to create a waveform in the time domain that happens to have that white noise spectrum.

The value of `magnitude` is used to determine the magnitude of each separate frequency sample
before the inverse transform is applied.

This noise generator is useful for plotting a graph of the prequency response of other digital filters, as its output can be fed to a digital filter, and the forward Fourier transform applied to the output samples to recover the output frequency spectrum of the filtered signals.

## White Gaussian noise generator

```
public static IEnumerable<double> WhiteNoise(int duration, double magnitude = 1.0)
```
This noise generator like the synthetic noise generator described above creates a sequence
of samples whose amplitude has a Gaussian probability distribution, just like background electrical noise in say a radio environment. As usual the `duration` parameter determines the total number of samples to be generated, while the `magnitude` parameter can be used to
scale the RMS amplitude of the gaussian noise waveform.

## Combining operators

```
public static IEnumerable<double> Sum(IEnumerable<double> src1, IEnumerable<double> src2)
public static IEnumerable<double> Difference(IEnumerable<double> src1, IEnumerable<double> src2)
public static IEnumerable<double> Product(IEnumerable<double> src1, IEnumerable<double> src2)
```
These operators combine two input sequences sample by sample to create a single combined output
sequence. For example, the `Sum` operator ensures that the Nth output sample is the sum of the two Nth input samples from the two source streams `src1` and `src2`.

The `Difference` operator subtracts the samples in sequence `src2` from the samples in `src1`.

The `Product` operator multiplies the corresponding samples in the two input sequences together.

## Example

The following code creates a waveform that is a sinewave at 400Hz added to a cosine wave at 600Hz, then the whole thing windowed in amplitude with what is called a Hann window after its inventor, Julius von Hann, this
being an effect where you multiply the whole waveform by a long slow raised cosine shape so that
the middle of the waveform is full amplitude but the waveform tapers away to nothing at the
start and end of the waveform sequence:

```
var sin400 = SignalSources.SineWave(400, 0, 2000, 8000, 1.0);
var cos600 = SignalSources.SineWave(600, 90, 2000, 8000, 1.0);
var window = SignalSources.RaisedCosine(8000, 8000, 1.0);
var result = SignalSources.Product(window, SignalSources.Sum(sin400, cos600));
```

