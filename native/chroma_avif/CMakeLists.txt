cmake_minimum_required(VERSION 3.16)
project(chromaavif LANGUAGES C CXX)

add_library(chromaavif SHARED "${CMAKE_CURRENT_LIST_DIR}/../chroma_avif.c")
target_include_directories(chromaavif PRIVATE "${AVIF_INCLUDE_DIR}")
target_link_libraries(
    chromaavif
    PRIVATE "${AVIF_LIBRARY}" "${DAV1D_LIBRARY}" "${AOM_LIBRARY}"
)
set_target_properties(
    chromaavif
    PROPERTIES LINKER_LANGUAGE CXX OUTPUT_NAME chromaavif C_VISIBILITY_PRESET hidden
)

if(NOT WIN32)
    find_package(Threads REQUIRED)
    target_link_libraries(chromaavif PRIVATE Threads::Threads m)
endif()

if(APPLE)
    set_target_properties(chromaavif PROPERTIES INSTALL_NAME_DIR "@rpath" BUILD_RPATH "@loader_path")
endif()
