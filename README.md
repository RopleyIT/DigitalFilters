# DigitalFilters
## Overview
This suite of libraries implements a (growing) set of classes and algorithms for creating, 
designing and experimenting with digital filters. In its current form, the library offers
the following capabilities:
- Generating Butterworth filter polynomials for low and high pass Butterworth filters of any order
- Implementing the bilinear Z transform so that analogue-equivalent filters such as the Butterworth
  filters spoken of above can be mapped to the digital filter domain
- Applying frequency warping so that the cut off frequencies for the digital filters are at the same
  frequency as their analogue counterparts
- Rendering the waveforms output from digital filters either on JPG or SVG files
## Footnote
Please note that this is ongoing work rather than a completed product in support. Once a comprehensive
set of library classes and methods has been assembled, documentation on how to use the classes will
also be created. It is intended that the library will support both IIR and FIR filters, with and 
without windowing, discrete/fast fourier transforms and their inverses, together with improved
graphical support for rendering frequency spectra, phase/frequency plots and waveforms.
