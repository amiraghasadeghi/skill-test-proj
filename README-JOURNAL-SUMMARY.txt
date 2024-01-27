Coding Strategy and Progress - Candidate Test MM Aviation

Date: 22/01/2024

---------Initial Setup:

Pulled the project onto my local machine and installed .NET Core 3.1 with the necessary SDKs. This foundational step ensured a smooth start to the development process.
Requirement Analysis: Analyzed the Candidate Test MM Aviation documentation to understand the task of outputting a string value based on aviation standards and rules. Used a Miro board for visualisation and breakdown of the requirements, including encoding, ranges, increments, and conditions associated with each WindData parameter.
Approach: Opted for a Test-Driven Development (TDD) approach for reliability and robustness. Envisioned the WindFormatter as a service provider with an interface to ensure scalability and adaptability.
Date: 23/01/2024

---------Test-Driven Development: 

Began coding by establishing a robust foundation for unit tests, starting with validating the non-nullity of the WindData model. Expanded the scope of unit tests parallel to the development of functional code, ensuring immediate validation for correctness and reliability.
Iterative Testing and Development: Continuously added test conditions for new scenarios, enhancing system robustness. Followed a cycle of writing minimal functional code, testing, and improving the code to pass tests.

Date: 26/01/2024 - 27/01/2024

---------Refactoring Journey: 

Post unit tests, embarked on comprehensive refactoring. Focused on enhancing maintainability, scalability, and overall quality by adhering to DRY and SOLID principles.
Implementing DRY and SOLID Principles: Eliminated redundant code, introduced a centralized ParsingValidator for parsing-related validations, and ensured that each class adhered to the SOLID principles, including using interfaces for dependency inversion.
Enhanced Error Handling: Implemented strategic exception handling, including specific handling for parsing errors with ParsingException.
Logging Integration: Added a centralized logging service, with considerations for both VM hosted applications and cloud-based services like Azure App Services.

---------Clash of Rules:

Encountered a clash between two rules regarding variable wind direction and missing values. Temporarily omitted the conflicting part pending further discussion and clarification.

---------Conclusion:

The journey involved careful planning, rigorous testing, and strategic refactoring. The project not only met the functional requirements but also aligned with best practices in software development, ensuring a reliable, maintainable, and scalable solution.
Reflective Note: This experience underscored the importance of a methodical approach, continuous testing, and adaptability in resolving complex software development challenges.

[FOR MORE INDEPTH JOURNAL PLEASE LOOK AT ATTACHED PDF FILE (JOURNAL)]