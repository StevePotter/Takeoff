Takeoff Data

This project contains the API to Takeoff's data model.  It does not include any concrete repository implementations, which is why it's mostly interfaces.  This allows the app to be flexible with its underlying data repository, making it easy to test and swap ORMs.

Business logic, web apps, and tools should use this library instead of interfacing directly with any databases.  Repositories are hooked up via IoC class.

