# DigitalFilters
## Overview
This suite of libraries implements a (growing) set of classes and algorithms for creating, 
designing and experimenting with digital filters. In its current form, the library offers
the following capabilities:
- Generating Butterworth filter polynomials for low and high pass Butterworth filters of any order
- Implementing the bilinear Z transform so that analogue-equivalent filters such as the
Butterworth filters spoken of above can be mapped to the digital filter domain
- Applying frequency pre-warping so that the cut off frequencies for the digital filters
are at the same frequency as their analogue counterparts
- Rendering the waveforms output from digital filters either on JPG or SVG files
- Performing forward and inverse fast Fourier transforms
- Creating waveforms or pulses of various shapes to feed through the filters or transforms

The documentation for these libraries will be found in the [GitHub wiki pages for this project](wiki/).
