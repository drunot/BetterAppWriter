cmake_minimum_required(VERSION 3.18)
set(CMAKE_GENERATOR_PLATFORM "Win32" CACHE STRING "" FORCE)
PROJECT(nlp VERSION 1.0.0)
# Add version number to nlp wrapper dll.
configure_file(
  ${CMAKE_CURRENT_SOURCE_DIR}/version.rc.in
  ${CMAKE_CURRENT_BINARY_DIR}/version.rc.in
  @ONLY)

file(
  GENERATE
  OUTPUT "${CMAKE_CURRENT_BINARY_DIR}/version.rc"
  INPUT "${CMAKE_CURRENT_BINARY_DIR}/version.rc.in")

ENABLE_LANGUAGE(C CXX RC)
ADD_LIBRARY(nlp SHARED nlp.cpp nlp.def empty.h hook_macro.h ${CMAKE_CURRENT_BINARY_DIR}/version.rc)

add_subdirectory(sharploader)