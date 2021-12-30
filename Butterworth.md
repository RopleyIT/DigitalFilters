# Analogue and Digital Filters

Analogue filters are used in analogue electronic circuits to filter out unwanted frequencies from signals, such as audio streams. In this form, they are usually implemented with capacitors, inductors and resistors, often with operational amplifiers at lower frequencies to preserve gain. By taking an analogue filter design and applying some standard transformations to it, it can be used to implement the equivalent digital filter, filtering a continuous,equally-spaced stream of samples in a computer, for example.

A number of well known analogue filter families exist, with names such as Butterworth, Bessel, Tchebyshev or Elliptical. The noteworthy characteristic of the best known of these, th Butterworth filter, is that it is 'maximally flat in the passband'. This means that over the range of frequencies it is not attenuating, the ratio of
input to output signal amplitude stays pretty constant. For each of these analogue filters, the rate at which they increase the
attenuation of the signal as the frequency deviates away from the passband is determined by
the 'order' of the filter. 

There is brilliant literature on the maths used to design analogue filters, such as the Butterworth filters, available on the Web. We are not going to reregurgitate that here, but some good links to introductory and topical material are given.

- Introduction to Laplacian mathematics as used in filter design:

  [Laplace transform and circuit theory](https://en.wikibooks.org/wiki/Circuit_Theory/Laplace_Transform)

- The maths behind the Butterworth filter in all its various forms:

  [Butterworth filters](https://www.electronicshub.org/butterworth-filter/)

## Digital Filters

Digital filters are applicable where an Analogue to Digital Converter is used to sample an input
signal at a regular rate known as the *sampling frequency*. These samples are captured as a stream of integers, and can be fed into one side of a digital filter, causing a new stream of
integers to come out the other side of the filter. The new stream has been filtered according to the design of the filter.

Digital filters come from two families - *Finite Impulse Response (FIR)* filters, 
and *Infinite Impulse Response (IIR)* filters. Digital filters created from an analogue filter
design, such as the Butterworth filter family, are nearly always IIR filters.

IIR filters are usually more complex to design than FIR, but they have a major advantage when
it comes to operational complexity. Very few multiplications and additions on sample streams need to be
performed to create each output sample from these filters. The equivalent FIR filters, though
simple to design, usually perform more computations per sample.

### Design steps for IIR filters

The details of how digital filters are derived from their analogue counterparts are given in depth [elsewhere](https://www.earlevel.com/main/2003/03/02/the-bilinear-z-transform/). In a nutshell however, the procedure goes through the following steps:

1. The analogue equivalent filter is designed, for example the Butterworth filters discussed in the links above;
2. The chosen cut-off frequency for the analogue filter (where the edge of the passband appears in the frequency spectrum) needs to be *pre-warped*. This is because the next step that translates the filter design to the digital domain distorts the frequency spectrum somewhat, so by pre-warping we arrange that the cut-off frequency is in the right place once that distortion happens;
3. The Bilinear-Z Transform is applied to the pre-warped analogue filter design;
4. The difference equation is derived from the resulting Z-domain transfer function, this being a direct representation of the multiplications, additions, and sample memory stores used in the
implementation.

The libraries in this software suite carry out all this work for you. You can tell them what order of Butterworth filter you want, and what cut-off frequency it should have. You can tell them what the sampling rate will be for the equivalent digital filter, and the library will do the pre-warping and give you back a working filter that implements the linear difference equation for the filter at the chosen sample rate.

### The `Butterworth` analogue filter design class

This class allows you to choose the order of your filter, the cut-off frequency, and whether it is a high or low pass filter. What you get back are the various Laplace complex polynomials needed to make your N-th order filter out of a set of consecutive 1st and 2nd order filter blocks.

| Method | Description |
| --- | --- |
| `Butterworth(int order, double cutOff, bool hiPass)` | Constructor for the filter object. The arguments are described below. |
| `order` | The order of the filter. The higher the order, the greater the filter's complexity, but the faster it rolls off outside the passband. |
| `cutOff` | The frequency at which the filter is attenuating by 3dB relative to the passband. Note that this frequence is in radians per second, not Hz. Multiply your Hz cutoff frequnecy by 2*PI to get radian frequency. |
| `hiPass`| Set to `true` for a high-pass filter, false for low-pass. |
| `IReadOnlyList<ComplexPoly> Polynomials { get; }` | The set of output Laplace polynomials used to build the filter. These will be fed to the class that generates the digital equivalent filter. |

### The `IFilter` interface

Since there are numerous other types of filter than just Butterworth, the `IFilter` interface is an abstraction that can be used to build a digital filter from any of them. Examples of other analogue filter types are Bessel, Tchebychev (two sub types) or Elliptic. In practice
the only benefit Bessel filters have is a linear (constant delay) phase response across the pass and transition bands. Since teh Bilinear Z transform distorts the filter's characteristic,
this benefit is lost. Hence it is mainly the other two types of filter that might be used
to design digital IIR filters.

| Member | Description |
| --- | --- |
| `HighPass` | The boolean flag indicating whether this is a high or low pass filter.|
| `Order` | The integer order of the filter chosen to achieve a particular rate of roll-off in the transition band. |
| `CutOff` | The -3dB frequency for the filter relative to the peak passband amplitude. |
| `Polynomials` | The set of Laplace 1st or 2nd order polynomials used to design and build the filter. Described above. |

### Creating an IIR filter

Once you have created your Butterworth or similar analogue filter design, you need to apply pre-warping
to its cutoff frequency, and then apply the Bilinear Z transform to create the digital filter.

To do this, you use the `IIRFilter` class. It in turn needs the set of polynomials generated by
the analog filter class, such as `Butterworth`. The methods and properties of the `IIRFilter` class
are described here.

| Member | Description |
| --- | --- |
| `IIRFilter(IFilter analogueFilter, double samplingRate, double gain = 1)` | Constructor. The parameters are described below. |
| `analogueFilter` | The analogue equivalent filter this is going to transform to a digital filter. |
| `samplingRate` | The rate at which samples will be fed through the filter. Needed to pre-warp correctly. |
| `gain` | Used to boost the output amplitude from the filter, if too attenuated. Note that this and the other two parameters are exposed as properties of the IIRFilter as well.|
| `List<IIRFilterStage> FilterStages` | The actual tap coefficients if you wish to hand craft your own fitler implementations. |
| `IEnumerable<double> Filter(IEnumerable<double> source)` | Use the instance of the filter held by this IIRFilter object. |
| `source` | The input stream of samples |
| Return value | Returns the filtered stream of samples. Can use multiple filters in cascade this way. |

### A worked example

Let's say we wish to design a filter to be used in a system that is processing input samples at
a rate of 2000 samples per second. The filter is to be a bandpass filter allowing waveform
frequencies through between 100 and 400 Hz. Outside this range, we require the signal to be
well attenuated.

For argument's sake, we are going to assume that our filter needs seventh order filter designs
in order to get a fast enough drop off in the transition band (the range of frequencies just to
either side of the passband in which the signal becomes rapidly more attenuated the more Hz you
get away from the cut-off frequencies).

To create the analogue filter, we would use the Butterworth filter class as follows:

```
Butterworth bw = new(7, 2 * Math.PI * 400, false);
Butterworth hw = new(7, 2 * Math.PI * 100, true);
```

This code creates a low pass filter with a cutoff frequency of 400Hz, that is 7th order. Note the frequency argument should be an angular frequency in radians per second rather than a straight frequency in Hz. Hence the multiplication by 2*PI.

The second line of code creates a high pass filter that attenuates frequencies lower than 100Hz.
It too has been chosen to be seventh order.

By using one of these filters to filter the output of the other, we create the bandpass filter we originally set as our goal.

These filters have each created a numer of what are called first or second order filter stages. With seventh order filters, this means three second order filter stages and one first order stage. Each of these filter stages is described by a complex polynomial of the Laplacian complex frequency variable 's'. They are found in the `Polynomials` property of the Butterworth object as a read only list of polynomials.

We are now going to use these arrays of polynomials to create the digital filters using the Bilinear Z transform with frequency pre-warping. For each of the filters described above, we do this by creating two IIRFilter objects as follows:

```
IIRFilter iir = new(bw, 2000);
IIRFilter hir = new(hw, 2000);
```

These lines of code have created two digital filters, one for each Butterworth filter, for use at a sampling
rate of 2 kHz. Note that the sampling frequency here is specified in Hz, not radians per second
as with the Butterworth filter objects, just to confuse you! Internally they capture the Laplacian polynomials, and using the sampling rate argument apply the frequency warping. They then figure out exactly what architecture to use to implement each digital filter.

The IIRFilter class can also be used as the filter instance itself. It takes an `IEnumerable<double>` as an input sample stream from which it pulls the samples at the 2kHz sampling rate, and it returns another IEnumerable<double> which provides the output sample stream. Note that there is no buffering or strict 2kHz timing enforced by this implementation. If you pull one sample from the output, it will cause one sample to be pulled from the source stream, meaning the flow rate is determined by the caller. 

Some sample code below shows the use of the above two filters to implement a bandpass structure. Note that the two filters are cascaded to apply both the high pass and the low pass filtering.

```
IEnumerable<double> sourceSamples = ? ? ? // Fetch raw samples from somewhere
IEnumerable<double> filteredSamples = hir.Filter(iir.Filter(sourceSamples));
```
