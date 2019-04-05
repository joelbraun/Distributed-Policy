package httpapi.authz

# HTTP API request
import input

default allow = false

# setting an arbitrary permission to check
requiredPerm = "viewSalary"

# Allow users to get their own salaries.
allow {
  input.permissions[_] = requiredPerm
}
