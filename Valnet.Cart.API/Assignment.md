# Valnet back-end test assignment

## Intro
A new cool team member was hired by company recently! Jack passed all interview rounds with amazing feedback, company
was so happy when he accepted the offer. Moreover, he was immediately available and joined the company on last monday!
Jack joined a project for a key e-commerce customer. There is a scrum methodology in use with releases every two weeks.
Project is undergoing a transformation phase from monolithic to microservices architecture. Jack, due to his wast
experience with microservices in e-commerce was given a task to implement new checkout service.

After 7 days of work in the company Jack has left, saying that a company of his dream made him a job offer.
Marko has implemented UI part given the definition below and told CTO yesterday that some of `API`s are ready, but with
issues. Demo to the customer is planned for the Friday (in 3 working days).

CTO asks You (as the top performer of the company) to pick up the task and complete it!

## Requirements

Please find below the `API` contract (in `Typescript`) that was discussed with Marko - front-end developer:
```
GET      <host>/api/{userId}/cart -> returns Cart
POST     <host>/api/{userId}/cart/items?prodyctId={productId}&quantity={quantity} -> add or update item in cart -> returns Cart
DELETE   <host>/api/{userId}/cart/items?prodyctId={productId} -> removes item from cart -> returns Cart
POST     <host>/api/{userId}/cart/markAsPaid -> marks current cart as paid -> returns empty body

export interface Cart {
   totalAmount: number;
   cartItems: CartItem[];
}

export interface CartItem {
   productId: number;
   price: number;
   quantity: number;
}
```

## Assignment:

1. Check the source code against API contract and list all functional gaps into `TODO.md`
   * [Optional] List all possible improvement in existing codebase
   * [Optional] Decompose tasks if required   
   * Estimate each of the list items in hours
   * Prioritize the list
2. Define "API endpoint is ready fro production" criteria in `TODO.md`
3. Implement as much as possible given the time constraint. Mark work-items as done in `TODO.md`  
   * Archive the results (pls remove any binaries), upload to google drive and send a link back to us
4. [Optional] Provide `README.md`
5. [Optional] Try not to spend more than 4 hours (there is no need to implement all API endpoints)

### Definition of done:

There is no goal to implement all APIs, however, assignment considered as done if:

1. There is a sorted and estimated list of gaps and improvements in TODO.me
2. Code quality is good: all `SOLID`, no `DRY` issues, debugged and tested
3. There is at least one `production ready` `REST API` endpoint (set of endpoints, depending on your definition
   of `production ready`)

### Additional requirements:

1. No authentication is OK
2. In memory DB (even using `List<T>`) is OK
3. You cannot change `IProductService` interface, but you can implement a stub for it
4. Use gitflow with local git repository
5. While submitting the results, pls mind the archive file-name (consider using your name as in CV) 

### Additional evaluation

Among two candidates with the same formal result, the one with the better score of following criterias would be considered:

1. Code quality, project structure, code style, application of `DDD`/`OOP`/`SOLID`/`DRY`/`KISS` -> readability, maintainability, simplicity
2. Similarity of candidate's `definition of done` and company one`s
3. Test coverage 
4. Ability to explain your decision
5. Cross platform support (ability to run the app on a linux env)