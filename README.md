# Crawl

Crawl is a command line tool used to extract large amounts of text from the Web for language analysis.

## Getting Started

### --url <string>

Specifies the starting URL to be crawled.

```
crawl --url https://www.w3.org/TR/html5/
```

### --depth <int>

Specifies the allowed depth level.

```
crawl --url https://www.w3.org/TR/html5/ --depth 7
```

### --verbose

If this flag is specified, the text is printed out to console during the crawl process.

```
crawl --url https://www.w3.org/TR/html5/ --verbose
```