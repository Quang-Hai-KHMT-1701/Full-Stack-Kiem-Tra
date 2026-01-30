/**
 * Auth utilities
 */
export const getToken = () => {
  return localStorage.getItem('token')
}

export const setToken = (token) => {
  localStorage.setItem('token', token)
}

export const removeToken = () => {
  localStorage.removeItem('token')
}

export const getUser = () => {
  const user = localStorage.getItem('user')
  if (user) {
    try {
      return JSON.parse(user)
    } catch (e) {
      return null
    }
  }
  return null
}

export const setUser = (user) => {
  localStorage.setItem('user', JSON.stringify(user))
}

export const removeUser = () => {
  localStorage.removeItem('user')
}

export const isAuthenticated = () => {
  return !!getToken()
}

export const clearAuth = () => {
  removeToken()
  removeUser()
}
