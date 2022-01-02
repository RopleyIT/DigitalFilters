# Windowing Functions

When performing digital Fourier transforms, discontinuities at each end 
of the waveform sequence being transformed cause the spectrum to be
distorted by spectral leakage. An FFT is really designed to generate
the spectrum of a periodic waveform with period equal to the duration
of the whole set of samples in the transform input. Hence a discontinuity
at the end of the sample set compared back to the first sample introduces
a step into the overall periodic waveform that causes the spectral
leakage.

To reduce the effect of this leakage, the amplitude of the whole sequence
of samples is tapered at each edge of the sample set so that the edge
samples are attenuated towards a zero value. The effect of this is to
reduce spectral leakage dramatically, but the shape of the window function
applied causes the amplitude of each frequency captured by the transform
to leak into adjacent frequency samples. Hence there is a tradeoff to be made
of local adjacent sample leakage versus widespread leakage many samples
away from the frequency of interest.

A number of window functions exist that give differing degrees of tradeoff,
from the Hamming window for example, which doubles the main lobe leakage
width in return for peak sidelobes around -40dB, up to windows like the
Dolph Chebyshev window that has a much wider main lobe, but all sidelobes
below -100dB.

For a more detailed foray into the world of window functions the
[Wikipedia entry](https://en.wikipedia.org/wiki/Window_function)
is a good starting point for more details and for
onward-linking references. Remember to donate to Wikipedia of course!

This library gives implementations of some of the more widely used window
functions. They should be combined with the input sample set for a Fourier
transform using the `SignalSource.Product` method before feeding the
time domain samples into the FFT to be mapped to the frequency domain.

## Hann window

```
double WindowFunction.Hann(int sampleIndex, int maxSamples)
```
This function provides the scaling factors for the Hann window. The Hann
window is a raised cosine window of width `maxSamples` for one full period.
It has a widening of the main lobe relative to the unwindowed case of 1.5,
but first sidelobes at -32dB and roll-off around 18dB per octave.

## Hamming window

```
double WindowFunction.Hamming(int sampleIndex, int maxSamples)
```
This window function slightly alters the Hann window function to remove
the first sidelobes of the Hann window spectrum. It has its highest sidelobes
at -43dB, but sidelobes far from the centre of the window's spectrum do not
fall away at 18dB per octave, but settle at around -60dB. The main lobe width
is also slightly narrower than Hann, at 1.368 times the rectangular
window.

## Blackman window

```
double WindowFunction.Blackman(int sampleIndex, int maxSamples)
```
By introducing an extra raised cosine component at half the period of the
Hann and Hamming windows, the Blackman window has its first sidelobes at -58dB
with an 18dB per octave roll-off from there. The penalty paid is the main lobe
spread is widened to 1.727 times the width of the main lobe for a rectangular
window.

## Nuttall and Blackman-Nuttall windows

```
double WindowFunction.BlackmanNuttall(int sampleIndex, int maxSamples)
double WindowFunction.Nuttall(int sampleIndex, int maxSamples)
```
The Nuttall window incorporates a third harmonic term in its sum of raised cosine
curves. Though this widens the main lobe to 2.021 times the width of the
rectangular window, the highest sidelobes appear at -93dB with an 18dB per octave
roll-off thereafter.

The Blackman-Nuttall window function slightly tweaks the coefficients for the
Nuttall window function so that the main lobe redueces slightly to 1.976 times the
rectangular window width, but the highest sidelobes are at -98dB.
