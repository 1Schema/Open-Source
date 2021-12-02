# Open-Source

This repository contains the 1Schema.com Code provided by Decia LLC as implemented as of late 2016 with minor updates to the export library in early 2017.

This code implements the designs specified in our [System Specification](./IPCOM000255833D.pdf) and our [Reporting Specification](./DeciaLLC_1Schema_Reporting.pdf).

Particularly, we provide the following libraries under the [MIT Licenese](./LICENSE): 
* **Common**: Contains StructuralMap, DependancyMap, and other base level resources
* **Domain**: Contains other base level resources
* **ChronometricValues**: Contains N-Dimensional matrix of time dimensions currently limited to two dimensions
* **CompoundUnits**: Contains a unit of measure that maintains the mathmatical correctness
* **Exports**: Contains both SQL and NoSQL exporters
* **Formulas**: Contains a forlula object that contains an expression tree and arguments that can be computed using the FormulaProcessingEngine
* **Reporting**: Contains a report object that contains nested elements that can be renedered using ReportRenderingEngine


Copyright © 2021 Decia LLC